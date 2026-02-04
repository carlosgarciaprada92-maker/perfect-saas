import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { Factura, FacturaItem, EstadoCartera, TipoPago } from '../models/factura.model';
import { Cliente } from '../models/cliente.model';
import { ProductsService } from './products.service';
import { StorageService } from './storage.service';
import { ConfigService } from './config.service';
import { seedFacturas } from './seed-data';

@Injectable({ providedIn: 'root' })
export class InvoicesService {
  private readonly storageKey = 'inv_facturas';
  private readonly invoicesSubject = new BehaviorSubject<Factura[]>(seedFacturas);

  readonly facturas$ = this.invoicesSubject.asObservable();

  constructor(
    private readonly storage: StorageService,
    private readonly products: ProductsService,
    private readonly config: ConfigService
  ) {
    const stored = this.storage.get(this.storageKey, seedFacturas);
    const normalized = stored.map((factura) => this.normalizeFactura(factura));
    this.invoicesSubject.next(normalized);
    this.refreshEstados();
  }

  get snapshot(): Factura[] {
    return this.invoicesSubject.value;
  }

  getById(id: string): Factura | undefined {
    return this.snapshot.find((factura) => factura.id === id);
  }

  createFactura(params: {
    cliente?: Cliente;
    clienteId?: string;
    items: FacturaItem[];
    tipoPago: TipoPago;
    impuestos?: number;
    plazoCreditoDiasUsado?: number;
  }): Factura {
    const subtotal = params.items.reduce((acc, item) => acc + item.totalLinea, 0);
    const impuestos = params.impuestos ?? 0;
    const total = subtotal + impuestos;
    const consecutivo = this.getNextConsecutive();
    const createdAt = new Date().toISOString();

    let fechaVencimiento: string | undefined;
    let plazoCreditoDiasUsado: number | undefined;
    if (params.tipoPago === 'CREDITO') {
      const dias = params.plazoCreditoDiasUsado ?? params.cliente?.plazoCreditoDias ?? 15;
      const dueDate = new Date(createdAt);
      dueDate.setDate(dueDate.getDate() + dias);
      fechaVencimiento = dueDate.toISOString();
      plazoCreditoDiasUsado = dias;
    }

    const pagada = params.tipoPago === 'CONTADO';
    const saldoPendiente = pagada ? 0 : total;

    const factura: Factura = {
      id: `fac-${crypto.randomUUID?.() ?? Math.random().toString(16).slice(2)}`,
      consecutivo,
      createdAt,
      clienteId: params.clienteId ?? params.cliente?.id,
      items: params.items,
      subtotal,
      impuestos,
      total,
      tipoPago: params.tipoPago,
      plazoCreditoDiasUsado,
      fechaVencimiento,
      estadoCartera: this.resolveEstado({
        tipoPago: params.tipoPago,
        fechaVencimiento,
        pagada,
        saldoPendiente
      }),
      pagada,
      saldoPendiente,
      fechaPago: pagada ? createdAt : undefined
    };

    const updated = [factura, ...this.snapshot];
    this.invoicesSubject.next(updated);
    this.storage.set(this.storageKey, updated);
    this.productsAdjustStock(params.items);
    return factura;
  }

  getCartera(): Factura[] {
    return this.snapshot.filter((factura) => factura.tipoPago === 'CREDITO');
  }

  refreshEstados(): void {
    const updated = this.snapshot.map((factura) => {
      return {
        ...factura,
        estadoCartera: this.resolveEstado(factura)
      };
    });
    this.invoicesSubject.next(updated);
    this.storage.set(this.storageKey, updated);
  }

  markAsPaid(id: string): void {
    const updated = this.snapshot.map((factura) => {
      if (factura.id !== id) {
        return factura;
      }
      const paidAt = new Date().toISOString();
      return {
        ...factura,
        pagada: true,
        saldoPendiente: 0,
        fechaPago: paidAt,
        estadoCartera: 'PAGADA' as EstadoCartera
      };
    });
    this.invoicesSubject.next(updated);
    this.storage.set(this.storageKey, updated);
  }

  registerPayment(id: string, amount: number): void {
    if (!amount || amount <= 0) {
      return;
    }
    const updated = this.snapshot.map((factura) => {
      if (factura.id !== id) {
        return factura;
      }
      const currentSaldo = factura.saldoPendiente ?? factura.total;
      const newSaldo = Math.max(0, currentSaldo - amount);
      const pagada = newSaldo === 0;
      const fechaPago = pagada ? new Date().toISOString() : factura.fechaPago;
      const estadoCartera = pagada
        ? ('PAGADA' as EstadoCartera)
        : this.resolveEstado({
            tipoPago: factura.tipoPago,
            fechaVencimiento: factura.fechaVencimiento,
            pagada,
            saldoPendiente: newSaldo
          }) ?? factura.estadoCartera;
      return {
        ...factura,
        saldoPendiente: newSaldo,
        pagada,
        fechaPago,
        estadoCartera
      };
    });
    this.invoicesSubject.next(updated);
    this.storage.set(this.storageKey, updated);
  }

  reset(data: Factura[] = seedFacturas): void {
    const normalized = data.map((factura) => this.normalizeFactura(factura));
    this.invoicesSubject.next(normalized);
    this.storage.set(this.storageKey, normalized);
    this.refreshEstados();
  }

  private resolveEstado(factura: {
    tipoPago: TipoPago;
    fechaVencimiento?: string;
    pagada?: boolean;
    saldoPendiente?: number;
  }): EstadoCartera | undefined {
    if (factura.tipoPago === 'CONTADO') {
      return 'PAGADA';
    }
    if (factura.pagada || (factura.saldoPendiente ?? 0) === 0) {
      return 'PAGADA';
    }
    if (!factura.fechaVencimiento) {
      return 'AL_DIA';
    }
    return this.calcularEstadoCartera(factura.fechaVencimiento);
  }

  private calcularEstadoCartera(fechaVencimiento: string): EstadoCartera {
    const threshold = this.config.snapshot.umbralProximoVencer;
    const vencimiento = new Date(fechaVencimiento).getTime();
    const ahora = Date.now();
    const diffDays = Math.ceil((vencimiento - ahora) / (24 * 60 * 60 * 1000));

    if (diffDays < 0) {
      return 'VENCIDO';
    }
    if (diffDays <= threshold) {
      return 'PROXIMO_VENCER';
    }
    return 'AL_DIA';
  }

  private getNextConsecutive(): string {
    const count = this.snapshot.length + 1;
    return `FAC-${count.toString().padStart(4, '0')}`;
  }

  private productsAdjustStock(items: FacturaItem[]): void {
    items.forEach((item) => this.products.adjustStock(item.productoId, -item.cantidad));
  }

  private normalizeFactura(factura: Factura): Factura {
    const tipoPago = factura.tipoPago;
    const pagada =
      factura.pagada ?? (tipoPago === 'CONTADO' ? true : false);
    const saldoPendiente =
      factura.saldoPendiente ??
      (tipoPago === 'CONTADO' ? 0 : factura.total);
    let plazoCreditoDiasUsado = factura.plazoCreditoDiasUsado;
    if (!plazoCreditoDiasUsado && factura.fechaVencimiento) {
      const created = new Date(factura.createdAt).getTime();
      const venc = new Date(factura.fechaVencimiento).getTime();
      const diffDays = Math.ceil((venc - created) / (24 * 60 * 60 * 1000));
      plazoCreditoDiasUsado = diffDays > 0 ? diffDays : undefined;
    }
    const estadoCartera = factura.estadoCartera ?? this.resolveEstado({
      tipoPago,
      fechaVencimiento: factura.fechaVencimiento,
      pagada,
      saldoPendiente
    });
    return {
      ...factura,
      pagada,
      saldoPendiente,
      estadoCartera,
      plazoCreditoDiasUsado
    };
  }
}
