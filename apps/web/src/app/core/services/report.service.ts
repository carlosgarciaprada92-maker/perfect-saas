import { Injectable } from '@angular/core';
import { combineLatest, Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { InvoicesService } from './invoices.service';
import { CustomersService } from './customers.service';
import { ConfigService } from './config.service';
import { Factura } from '../models/factura.model';
import { Cliente } from '../models/cliente.model';

export type EstadoCredito = 'PENDIENTE' | 'AL_DIA' | 'POR_VENCER' | 'VENCIDO' | 'PAGADA';

export interface CreditRow {
  factura: Factura;
  cliente?: Cliente;
  estado: EstadoCredito;
  daysToDue: number;
  daysOverdue: number;
}

export interface TopClienteRow {
  cliente: Cliente;
  total: number;
  count: number;
  promedio: number;
  creditoPct: number;
  contadoPct: number;
  creditoTotal: number;
  contadoTotal: number;
}

export interface InactiveClienteRow {
  cliente: Cliente;
  lastPurchase?: string;
  daysInactive: number;
  totalHistorico: number;
}

export interface OverdueClienteRow {
  cliente: Cliente;
  facturas: CreditRow[];
  facturasVencidas: number;
  saldoVencido: number;
  maxDiasVencidos: number;
  ultimoVencimiento?: string;
  facturaMasVencida?: CreditRow;
}

export interface VentasResumen {
  totalVentas: number;
  totalCredito: number;
  totalContado: number;
  totalCarteraPendiente: number;
  facturasVencidas: number;
  facturasPorVencer: number;
}

export interface VentasPorDiaRow {
  date: string;
  total: number;
  count: number;
  creditoTotal: number;
  contadoTotal: number;
}

export interface VentasTipoRow {
  tipo: string;
  total: number;
  count: number;
}

export interface DashboardFilters {
  rangeDays: number;
  umbralDias: number;
  clienteId?: string | null;
  includePagadas: boolean;
  inactiveDays: number;
}

export interface DashboardData {
  kpis: {
    totalPendiente: number;
    facturasVencidas: number;
    facturasPorVencer: number;
  };
  porVencer: CreditRow[];
  vencidos: CreditRow[];
  clientesVencidos: OverdueClienteRow[];
  topClientes: TopClienteRow[];
  inactivos: InactiveClienteRow[];
  resumen: VentasResumen;
}

export interface CarteraReportFilters {
  rangeDays: number;
  umbralDias: number;
  clienteId?: string | null;
  includePagadas: boolean;
  estado?: EstadoCredito | 'TODOS';
  search?: string;
}

export interface CarteraReportData {
  rows: CreditRow[];
  totalPendiente: number;
}

export interface VentasReportFilters {
  rangeDays: number;
}

export interface VentasReportData {
  porDia: VentasPorDiaRow[];
  porTipo: VentasTipoRow[];
  resumen: VentasResumen;
}

export interface ClientesReportFilters {
  rangeDays: number;
  inactiveDays: number;
}

export interface ClientesReportData {
  topClientes: TopClienteRow[];
  inactivos: InactiveClienteRow[];
}

@Injectable({ providedIn: 'root' })
export class ReportService {
  private readonly base$: Observable<{
    facturas: Factura[];
    clientes: Cliente[];
    config: unknown;
  }>;

  constructor(
    private readonly invoices: InvoicesService,
    private readonly customers: CustomersService,
    private readonly config: ConfigService
  ) {
    this.base$ = combineLatest([
      this.invoices.facturas$,
      this.customers.clientes$,
      this.config.config$
    ]).pipe(map(([facturas, clientes, config]) => ({ facturas, clientes, config })));
  }

  dashboard$(filters$: Observable<DashboardFilters>): Observable<DashboardData> {
    return combineLatest([this.base$, filters$]).pipe(
      map(([data, filters]) => this.buildDashboard(data.facturas, data.clientes, filters))
    );
  }

  carteraReport$(filters$: Observable<CarteraReportFilters>): Observable<CarteraReportData> {
    return combineLatest([this.base$, filters$]).pipe(
      map(([data, filters]) => this.buildCarteraReport(data.facturas, data.clientes, filters))
    );
  }

  ventasReport$(filters$: Observable<VentasReportFilters>): Observable<VentasReportData> {
    return combineLatest([this.base$, filters$]).pipe(
      map(([data, filters]) => this.buildVentasReport(data.facturas, filters.rangeDays))
    );
  }

  clientesReport$(filters$: Observable<ClientesReportFilters>): Observable<ClientesReportData> {
    return combineLatest([this.base$, filters$]).pipe(
      map(([data, filters]) => this.buildClientesReport(data.facturas, data.clientes, filters))
    );
  }

  private buildDashboard(facturas: Factura[], clientes: Cliente[], filters: DashboardFilters): DashboardData {
    const creditRows = this.buildCreditRows(facturas, clientes, filters);
    const porVencer = creditRows
      .filter((row) => row.estado === 'POR_VENCER')
      .sort((a, b) => a.daysToDue - b.daysToDue);
    const vencidos = creditRows
      .filter((row) => row.estado === 'VENCIDO')
      .sort((a, b) => b.daysOverdue - a.daysOverdue);

    const totalPendiente = creditRows.reduce((acc, row) => acc + (row.factura.saldoPendiente ?? 0), 0);
    const facturasVencidas = vencidos.length;
    const facturasPorVencer = porVencer.length;

    const resumen = this.buildVentasResumen(facturas, filters.rangeDays, filters.umbralDias);

    return {
      kpis: {
        totalPendiente,
        facturasVencidas,
        facturasPorVencer
      },
      porVencer,
      vencidos,
      clientesVencidos: this.buildClientesVencidos(creditRows),
      topClientes: this.buildTopClientes(facturas, clientes, filters.rangeDays),
      inactivos: this.buildInactivos(facturas, clientes, filters.inactiveDays),
      resumen
    };
  }

  private buildCarteraReport(
    facturas: Factura[],
    clientes: Cliente[],
    filters: CarteraReportFilters
  ): CarteraReportData {
    let rows = this.buildCreditRows(facturas, clientes, {
      rangeDays: filters.rangeDays,
      umbralDias: filters.umbralDias,
      clienteId: filters.clienteId,
      includePagadas: filters.includePagadas,
      inactiveDays: 30
    });

    if (filters.estado && filters.estado !== 'TODOS') {
      rows = rows.filter((row) => row.estado === filters.estado);
    }

    const search = filters.search?.toLowerCase().trim();
    if (search) {
      rows = rows.filter((row) => {
        const cliente = row.cliente?.nombre.toLowerCase() ?? '';
        const factura = row.factura.consecutivo.toLowerCase();
        return cliente.includes(search) || factura.includes(search);
      });
    }

    const totalPendiente = rows.reduce((acc, row) => acc + (row.factura.saldoPendiente ?? 0), 0);

    return { rows, totalPendiente };
  }

  private buildVentasReport(facturas: Factura[], rangeDays: number): VentasReportData {
    const resumen = this.buildVentasResumen(facturas, rangeDays, this.config.snapshot.umbralProximoVencer);
    const ventas = facturas.filter((factura) => this.inRange(factura.createdAt, rangeDays));
    const mapPorDia = new Map<string, VentasPorDiaRow>();

    ventas.forEach((factura) => {
      const dateKey = factura.createdAt.slice(0, 10);
      const entry = mapPorDia.get(dateKey) ?? {
        date: dateKey,
        total: 0,
        count: 0,
        creditoTotal: 0,
        contadoTotal: 0
      };
      entry.total += factura.total;
      entry.count += 1;
      if (factura.tipoPago === 'CREDITO') {
        entry.creditoTotal += factura.total;
      } else {
        entry.contadoTotal += factura.total;
      }
      mapPorDia.set(dateKey, entry);
    });

    const porDia = Array.from(mapPorDia.values()).sort((a, b) => (a.date < b.date ? 1 : -1));

    const porTipo: VentasTipoRow[] = [
      {
        tipo: 'CONTADO',
        total: ventas.filter((f) => f.tipoPago === 'CONTADO').reduce((acc, f) => acc + f.total, 0),
        count: ventas.filter((f) => f.tipoPago === 'CONTADO').length
      },
      {
        tipo: 'CREDITO',
        total: ventas.filter((f) => f.tipoPago === 'CREDITO').reduce((acc, f) => acc + f.total, 0),
        count: ventas.filter((f) => f.tipoPago === 'CREDITO').length
      }
    ];

    return { porDia, porTipo, resumen };
  }

  private buildClientesReport(
    facturas: Factura[],
    clientes: Cliente[],
    filters: ClientesReportFilters
  ): ClientesReportData {
    return {
      topClientes: this.buildTopClientes(facturas, clientes, filters.rangeDays),
      inactivos: this.buildInactivos(facturas, clientes, filters.inactiveDays)
    };
  }

  private buildCreditRows(
    facturas: Factura[],
    clientes: Cliente[],
    filters: DashboardFilters
  ): CreditRow[] {
    const now = Date.now();
    const clientesMap = new Map(clientes.map((cliente) => [cliente.id, cliente]));

    return facturas
      .filter((factura) => factura.tipoPago === 'CREDITO')
      .filter((factura) => (filters.includePagadas ? true : (factura.saldoPendiente ?? 0) > 0))
      .filter((factura) => (filters.clienteId ? factura.clienteId === filters.clienteId : true))
      .filter((factura) => this.inRange(factura.createdAt, filters.rangeDays))
      .map((factura) => {
        const estado = this.resolveEstado(factura, filters.umbralDias);
        const diff = factura.fechaVencimiento
          ? Math.ceil((new Date(factura.fechaVencimiento).getTime() - now) / 86400000)
          : 0;
        return {
          factura,
          cliente: factura.clienteId ? clientesMap.get(factura.clienteId) : undefined,
          estado,
          daysToDue: diff > 0 ? diff : 0,
          daysOverdue: diff < 0 ? Math.abs(diff) : 0
        };
      });
  }

  private buildTopClientes(facturas: Factura[], clientes: Cliente[], rangeDays: number): TopClienteRow[] {
    const ventas = facturas.filter((factura) => this.inRange(factura.createdAt, rangeDays));
    const mapClientes = new Map<string, TopClienteRow>();
    const clientesMap = new Map(clientes.map((cliente) => [cliente.id, cliente]));

    ventas.forEach((factura) => {
      if (!factura.clienteId) {
        return;
      }
      const cliente = clientesMap.get(factura.clienteId);
      if (!cliente) {
        return;
      }
      const entry = mapClientes.get(factura.clienteId) ?? {
        cliente,
        total: 0,
        count: 0,
        promedio: 0,
        creditoPct: 0,
        contadoPct: 0,
        creditoTotal: 0,
        contadoTotal: 0
      };
      entry.total += factura.total;
      entry.count += 1;
      if (factura.tipoPago === 'CREDITO') {
        entry.creditoTotal += factura.total;
      } else {
        entry.contadoTotal += factura.total;
      }
      mapClientes.set(factura.clienteId, entry);
    });

    const rows = Array.from(mapClientes.values()).map((row) => {
      const total = row.total || 1;
      const creditoPct = (row.creditoTotal / total) * 100;
      const contadoPct = (row.contadoTotal / total) * 100;
      return {
        ...row,
        promedio: row.total / row.count,
        creditoPct,
        contadoPct
      };
    });

    return rows.sort((a, b) => b.total - a.total);
  }

  private buildInactivos(facturas: Factura[], clientes: Cliente[], inactiveDays: number): InactiveClienteRow[] {
    const now = Date.now();
    const mapUltimaCompra = new Map<string, { last?: string; total: number }>();

    facturas.forEach((factura) => {
      if (!factura.clienteId) {
        return;
      }
      const entry = mapUltimaCompra.get(factura.clienteId) ?? { last: undefined, total: 0 };
      entry.total += factura.total;
      if (!entry.last || new Date(factura.createdAt).getTime() > new Date(entry.last).getTime()) {
        entry.last = factura.createdAt;
      }
      mapUltimaCompra.set(factura.clienteId, entry);
    });

    return clientes
      .map((cliente) => {
        const entry = mapUltimaCompra.get(cliente.id);
        const lastDate = entry?.last ?? cliente.createdAt;
        const lastTime = lastDate ? new Date(lastDate).getTime() : 0;
        const daysInactive = lastTime ? Math.floor((now - lastTime) / 86400000) : inactiveDays + 1;
        return {
          cliente,
          lastPurchase: lastDate,
          daysInactive,
          totalHistorico: entry?.total ?? 0
        };
      })
      .filter((row) => row.daysInactive >= inactiveDays)
      .sort((a, b) => b.daysInactive - a.daysInactive);
  }

  private buildClientesVencidos(creditRows: CreditRow[]): OverdueClienteRow[] {
    const vencidos = creditRows.filter((row) => row.estado === 'VENCIDO' && (row.factura.saldoPendiente ?? 0) > 0);
    const map = new Map<string, OverdueClienteRow>();

    vencidos.forEach((row) => {
      if (!row.cliente) {
        return;
      }
      const current = map.get(row.cliente.id) ?? {
        cliente: row.cliente,
        facturas: [],
        facturasVencidas: 0,
        saldoVencido: 0,
        maxDiasVencidos: 0,
        ultimoVencimiento: undefined,
        facturaMasVencida: undefined
      };

      current.facturas.push(row);
      current.facturasVencidas += 1;
      current.saldoVencido += row.factura.saldoPendiente ?? 0;
      current.maxDiasVencidos = Math.max(current.maxDiasVencidos, row.daysOverdue);

      const vencimiento = row.factura.fechaVencimiento;
      if (vencimiento) {
        if (!current.ultimoVencimiento || new Date(vencimiento) > new Date(current.ultimoVencimiento)) {
          current.ultimoVencimiento = vencimiento;
        }
      }

      if (!current.facturaMasVencida || row.daysOverdue > current.facturaMasVencida.daysOverdue) {
        current.facturaMasVencida = row;
      }

      map.set(row.cliente.id, current);
    });

    return Array.from(map.values()).sort((a, b) => b.saldoVencido - a.saldoVencido);
  }

  private buildVentasResumen(facturas: Factura[], rangeDays: number, umbralDias: number): VentasResumen {
    const ventas = facturas.filter((factura) => this.inRange(factura.createdAt, rangeDays));
    const totalVentas = ventas.reduce((acc, factura) => acc + factura.total, 0);
    const totalCredito = ventas
      .filter((factura) => factura.tipoPago === 'CREDITO')
      .reduce((acc, factura) => acc + factura.total, 0);
    const totalContado = ventas
      .filter((factura) => factura.tipoPago === 'CONTADO')
      .reduce((acc, factura) => acc + factura.total, 0);

    const creditRows = ventas
      .filter((factura) => factura.tipoPago === 'CREDITO')
      .map((factura) => this.resolveEstado(factura, umbralDias));

    return {
      totalVentas,
      totalCredito,
      totalContado,
      totalCarteraPendiente: ventas
        .filter((factura) => factura.tipoPago === 'CREDITO')
        .reduce((acc, factura) => acc + (factura.saldoPendiente ?? 0), 0),
      facturasVencidas: creditRows.filter((estado) => estado === 'VENCIDO').length,
      facturasPorVencer: creditRows.filter((estado) => estado === 'POR_VENCER').length
    };
  }

  private resolveEstado(factura: Factura, umbralDias: number): EstadoCredito {
    const saldo = factura.saldoPendiente ?? 0;
    if (saldo === 0) {
      return 'PAGADA';
    }
    if (!factura.fechaVencimiento) {
      return 'PENDIENTE';
    }
    const diff = Math.ceil(
      (new Date(factura.fechaVencimiento).getTime() - Date.now()) / 86400000
    );
    if (diff < 0) {
      return 'VENCIDO';
    }
    if (diff <= umbralDias) {
      return 'POR_VENCER';
    }
    return 'AL_DIA';
  }

  private inRange(createdAt: string, rangeDays: number): boolean {
    if (!rangeDays) {
      return true;
    }
    const now = Date.now();
    const date = new Date(createdAt).getTime();
    return date >= now - rangeDays * 86400000;
  }
}
