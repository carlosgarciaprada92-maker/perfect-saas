import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { Cliente } from '../models/cliente.model';
import { StorageService } from './storage.service';
import { seedClientes } from './seed-data';

@Injectable({ providedIn: 'root' })
export class CustomersService {
  private readonly storageKey = 'inv_clientes';
  private readonly customersSubject = new BehaviorSubject<Cliente[]>(seedClientes);

  readonly clientes$ = this.customersSubject.asObservable();

  constructor(private readonly storage: StorageService) {
    const stored = this.storage.get(this.storageKey, seedClientes);
    this.customersSubject.next(stored);
    this.persist();
  }

  get snapshot(): Cliente[] {
    return this.customersSubject.value;
  }

  getById(id: string): Cliente | undefined {
    return this.snapshot.find((cliente) => cliente.id === id);
  }

  create(data: Omit<Cliente, 'id'>): Cliente {
    const cliente: Cliente = {
      ...data,
      id: `cli-${crypto.randomUUID?.() ?? Math.random().toString(16).slice(2)}`,
      createdAt: data.createdAt ?? new Date().toISOString()
    };
    const updated = [cliente, ...this.snapshot];
    this.customersSubject.next(updated);
    this.persist();
    return cliente;
  }

  update(id: string, changes: Partial<Cliente>): Cliente | undefined {
    let updatedItem: Cliente | undefined;
    const updated = this.snapshot.map((item) => {
      if (item.id === id) {
        updatedItem = { ...item, ...changes };
        return updatedItem;
      }
      return item;
    });
    this.customersSubject.next(updated);
    this.persist();
    return updatedItem;
  }

  remove(id: string): void {
    const updated = this.snapshot.filter((item) => item.id !== id);
    this.customersSubject.next(updated);
    this.persist();
  }

  reset(data: Cliente[] = seedClientes): void {
    this.customersSubject.next(data);
    this.persist();
  }

  private persist(): void {
    this.storage.set(this.storageKey, this.snapshot);
  }
}
