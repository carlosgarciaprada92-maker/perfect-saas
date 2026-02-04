import { Injectable } from '@angular/core';
import { buildSeedData, DEFAULT_DEMO_SEED } from './seed-data';
import { ProductsService } from './products.service';
import { CustomersService } from './customers.service';
import { InvoicesService } from './invoices.service';
import { InventoryService } from './inventory.service';

@Injectable({ providedIn: 'root' })
export class DemoDataService {
  resetToDefault(): void {
    this.applySeed(DEFAULT_DEMO_SEED);
  }

  regenerate(): void {
    this.applySeed(Date.now());
  }

  private applySeed(seed: number): void {
    const data = buildSeedData(seed);
    this.products.reset(data.productos);
    this.customers.reset(data.clientes);
    this.invoices.reset(data.facturas);
    this.inventory.reset(data.movimientos);
  }

  constructor(
    private readonly products: ProductsService,
    private readonly customers: CustomersService,
    private readonly invoices: InvoicesService,
    private readonly inventory: InventoryService
  ) {}
}
