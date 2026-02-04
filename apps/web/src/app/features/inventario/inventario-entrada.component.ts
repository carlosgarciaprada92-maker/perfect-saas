import { Component } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { AsyncPipe, DatePipe } from '@angular/common';
import { AutoCompleteModule } from 'primeng/autocomplete';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { MessageService } from 'primeng/api';
import { ProductsService } from '../../core/services/products.service';
import { InventoryService } from '../../core/services/inventory.service';
import { PageHeaderComponent } from '../../shared/components/page-header.component';
import { TranslateModule } from '@ngx-translate/core';
import { Producto } from '../../core/models/producto.model';
import { RolesService } from '../../core/services/roles.service';
import { AuthService } from '../../core/services/auth.service';
import { PermissionKey } from '../../core/models/app-module.model';

@Component({
  selector: 'app-inventario-entrada',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    AsyncPipe,
    DatePipe,
    AutoCompleteModule,
    InputNumberModule,
    InputTextModule,
    ButtonModule,
    TableModule,
    PageHeaderComponent,
    TranslateModule
  ],
  templateUrl: './inventario-entrada.component.html'
})
export class InventarioEntradaComponent {
  readonly productos$;
  readonly movimientos$;
  suggestions: Producto[] = [];

  readonly form;

  constructor(
    private readonly fb: FormBuilder,
    private readonly products: ProductsService,
    private readonly inventory: InventoryService,
    private readonly messages: MessageService,
    private readonly roles: RolesService,
    private readonly auth: AuthService
  ) {
    this.productos$ = this.products.productos$;
    this.movimientos$ = this.inventory.movimientos$;
    this.form = this.fb.group({
      producto: [null as Producto | null, Validators.required],
      cantidad: [1, [Validators.required, Validators.min(1)]],
      motivo: ['Entrada manual']
    });
  }

  searchProductos(event: { query: string }): void {
    const query = event.query.toLowerCase();
    this.suggestions = this.products.snapshot.filter((item) =>
      item.nombre.toLowerCase().includes(query)
    );
  }

  registrarEntrada(): void {
    if (!this.can('inventario:entrada')) {
      return;
    }
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const { producto, cantidad, motivo } = this.form.value;
    this.inventory.addEntrada(producto!.id, cantidad!, motivo ?? undefined);
    this.messages.add({
      severity: 'info',
      summary: 'Inventario actualizado',
      detail: 'Entrada registrada'
    });
    this.form.reset({ producto: null, cantidad: 1, motivo: 'Entrada manual' });
  }

  getProductoNombre(id: string): string {
    return this.products.getById(id)?.nombre ?? '-';
  }

  can(permission: PermissionKey): boolean {
    const user = this.auth.snapshot;
    if (!user) {
      return false;
    }
    return this.roles.hasPermission(user, permission);
  }
}
