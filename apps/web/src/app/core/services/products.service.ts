import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { Producto } from '../models/producto.model';
import { StorageService } from './storage.service';
import { seedProductos } from './seed-data';

@Injectable({ providedIn: 'root' })
export class ProductsService {
  private readonly storageKey = 'inv_productos';
  private readonly productsSubject = new BehaviorSubject<Producto[]>(seedProductos);

  readonly productos$ = this.productsSubject.asObservable();

  constructor(private readonly storage: StorageService) {
    const stored = this.storage.get(this.storageKey, seedProductos);
    this.productsSubject.next(stored);
    this.persist();
  }

  get snapshot(): Producto[] {
    return this.productsSubject.value;
  }

  getById(id: string): Producto | undefined {
    return this.snapshot.find((item) => item.id === id);
  }

  create(data: Omit<Producto, 'id' | 'createdAt'>): Producto {
    const producto: Producto = {
      ...data,
      id: `prod-${crypto.randomUUID?.() ?? Math.random().toString(16).slice(2)}`,
      createdAt: new Date().toISOString()
    };
    const updated = [producto, ...this.snapshot];
    this.productsSubject.next(updated);
    this.persist();
    return producto;
  }

  update(id: string, changes: Partial<Producto>): Producto | undefined {
    let updatedItem: Producto | undefined;
    const updated = this.snapshot.map((item) => {
      if (item.id === id) {
        updatedItem = { ...item, ...changes };
        return updatedItem;
      }
      return item;
    });
    this.productsSubject.next(updated);
    this.persist();
    return updatedItem;
  }

  remove(id: string): void {
    const updated = this.snapshot.filter((item) => item.id !== id);
    this.productsSubject.next(updated);
    this.persist();
  }

  adjustStock(id: string, delta: number): void {
    const item = this.getById(id);
    if (!item) {
      return;
    }
    this.update(id, { stockActual: Math.max(0, item.stockActual + delta) });
  }

  reset(data: Producto[] = seedProductos): void {
    this.productsSubject.next(data);
    this.persist();
  }

  private persist(): void {
    this.storage.set(this.storageKey, this.snapshot);
  }
}
