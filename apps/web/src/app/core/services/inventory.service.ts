import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { MovimientoInventario } from '../models/movimiento-inventario.model';
import { ProductsService } from './products.service';
import { StorageService } from './storage.service';
import { seedMovimientos } from './seed-data';

@Injectable({ providedIn: 'root' })
export class InventoryService {
  private readonly storageKey = 'inv_movimientos';
  private readonly movimientosSubject = new BehaviorSubject<MovimientoInventario[]>(seedMovimientos);

  readonly movimientos$ = this.movimientosSubject.asObservable();

  constructor(
    private readonly storage: StorageService,
    private readonly products: ProductsService
  ) {
    const stored = this.storage.get(this.storageKey, seedMovimientos);
    this.movimientosSubject.next(stored);
    this.storage.set(this.storageKey, stored);
  }

  addEntrada(productoId: string, cantidad: number, motivo?: string): MovimientoInventario {
    const movimiento: MovimientoInventario = {
      id: `mov-${crypto.randomUUID?.() ?? Math.random().toString(16).slice(2)}`,
      productoId,
      tipo: 'ENTRADA',
      cantidad,
      motivo,
      createdAt: new Date().toISOString()
    };
    const updated = [movimiento, ...this.movimientosSubject.value];
    this.movimientosSubject.next(updated);
    this.storage.set(this.storageKey, updated);
    this.products.adjustStock(productoId, cantidad);
    return movimiento;
  }

  reset(data: MovimientoInventario[] = seedMovimientos): void {
    this.movimientosSubject.next(data);
    this.storage.set(this.storageKey, data);
  }
}
