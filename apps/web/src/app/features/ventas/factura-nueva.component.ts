import { Component } from '@angular/core';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { AsyncPipe, CurrencyPipe, NgIf } from '@angular/common';
import { AutoCompleteModule } from 'primeng/autocomplete';
import { SelectModule } from 'primeng/select';
import { InputNumberModule } from 'primeng/inputnumber';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { MessageService } from 'primeng/api';
import { ProductsService } from '../../core/services/products.service';
import { CustomersService } from '../../core/services/customers.service';
import { InvoicesService } from '../../core/services/invoices.service';
import { AuthService } from '../../core/services/auth.service';
import { PageHeaderComponent } from '../../shared/components/page-header.component';
import { TranslateModule } from '@ngx-translate/core';
import { Producto } from '../../core/models/producto.model';
import { FacturaItem, TipoPago } from '../../core/models/factura.model';
import { Cliente } from '../../core/models/cliente.model';
import { Router } from '@angular/router';

@Component({
  selector: 'app-factura-nueva',
  standalone: true,
  imports: [
    FormsModule,
    ReactiveFormsModule,
    AsyncPipe,
    NgIf,
    CurrencyPipe,
    AutoCompleteModule,
    SelectModule,
    InputNumberModule,
    ButtonModule,
    TableModule,
    PageHeaderComponent,
    TranslateModule
  ],
  templateUrl: './factura-nueva.component.html'
})
export class FacturaNuevaComponent {
  readonly productos$;
  readonly clientes$;

  suggestions: Producto[] = [];
  selectedProducto: Producto | null = null;
  items: FacturaItem[] = [];

  readonly form;
  readonly plazoOptions = [
    { label: '8 dias', value: 8 },
    { label: '15 dias', value: 15 },
    { label: '30 dias', value: 30 },
    { label: 'Otro', value: 'OTRO' }
  ];

  constructor(
    private readonly fb: FormBuilder,
    private readonly products: ProductsService,
    private readonly customers: CustomersService,
    private readonly invoices: InvoicesService,
    private readonly auth: AuthService,
    private readonly messages: MessageService,
    private readonly router: Router
  ) {
    this.productos$ = this.products.productos$;
    this.clientes$ = this.customers.clientes$;
    this.form = this.fb.group({
      tipoPago: ['CONTADO' as TipoPago, Validators.required],
      clienteId: [''],
      plazoSeleccion: [15 as number | string],
      plazoOtro: [15]
    });

    this.form.get('clienteId')?.valueChanges.subscribe((clienteId) => {
      if (!clienteId) {
        return;
      }
      const cliente = this.customers.getById(clienteId);
      if (cliente) {
        this.form.patchValue({
          plazoSeleccion: cliente.plazoCreditoDias,
          plazoOtro: cliente.plazoCreditoDias
        }, { emitEvent: false });
      }
    });

    this.form.get('tipoPago')?.valueChanges.subscribe((tipo) => {
      if (tipo !== 'CREDITO') {
        this.form.patchValue({ clienteId: '', plazoSeleccion: 15, plazoOtro: 15 }, { emitEvent: false });
      }
    });
  }

  get canEditPrice(): boolean {
    return this.auth.snapshot?.rol === 'ADMIN' || this.auth.snapshot?.rol === 'CAJA';
  }

  get isPlazoOtro(): boolean {
    return this.form.get('plazoSeleccion')?.value === 'OTRO';
  }

  searchProductos(event: { query: string }): void {
    const query = event.query.toLowerCase();
    this.suggestions = this.products.snapshot.filter((item) =>
      item.nombre.toLowerCase().includes(query) ||
      item.sku?.toLowerCase().includes(query)
    );
  }

  addProducto(): void {
    if (!this.selectedProducto) {
      return;
    }
    const existing = this.items.find((item) => item.productoId === this.selectedProducto!.id);
    if (existing) {
      existing.cantidad += 1;
      existing.totalLinea = existing.cantidad * existing.precioUnitario;
    } else {
      this.items.push({
        productoId: this.selectedProducto.id,
        nombreProducto: this.selectedProducto.nombre,
        cantidad: 1,
        precioUnitario: this.selectedProducto.precioBase,
        totalLinea: this.selectedProducto.precioBase
      });
    }
    this.selectedProducto = null;
  }

  removeItem(item: FacturaItem): void {
    this.items = this.items.filter((line) => line !== item);
  }

  updateLine(item: FacturaItem): void {
    item.totalLinea = item.cantidad * item.precioUnitario;
  }

  get subtotal(): number {
    return this.items.reduce((acc, item) => acc + item.totalLinea, 0);
  }

  guardar(): void {
    if (this.items.length === 0) {
      this.messages.add({ severity: 'warn', summary: 'Sin items', detail: 'Agrega productos' });
      return;
    }
    const tipoPago = this.form.value.tipoPago as TipoPago;
    let cliente: Cliente | undefined;
    let plazoCreditoDiasUsado: number | undefined;
    if (tipoPago === 'CREDITO') {
      const clienteId = this.form.value.clienteId;
      cliente = this.customers.getById(clienteId || '');
      if (!cliente) {
        this.messages.add({ severity: 'warn', summary: 'Cliente requerido', detail: 'Selecciona cliente' });
        return;
      }
      const seleccion = this.form.get('plazoSeleccion')?.value;
      if (seleccion === 'OTRO') {
        plazoCreditoDiasUsado = Number(this.form.value.plazoOtro || 0);
      } else {
        plazoCreditoDiasUsado = Number(seleccion || 0);
      }
      if (!plazoCreditoDiasUsado || plazoCreditoDiasUsado <= 0) {
        this.messages.add({ severity: 'warn', summary: 'Plazo requerido', detail: 'Define el plazo' });
        return;
      }
    }

    const factura = this.invoices.createFactura({
      items: this.items,
      tipoPago,
      cliente,
      clienteId: cliente?.id,
      plazoCreditoDiasUsado
    });
    this.messages.add({ severity: 'info', summary: 'Factura creada', detail: factura.consecutivo });
    this.router.navigate(['/app/ventas/facturas', factura.id]);
  }
}
