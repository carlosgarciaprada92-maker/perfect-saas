import { Component, OnInit } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { InputNumberModule } from 'primeng/inputnumber';
import { ButtonModule } from 'primeng/button';
import { CheckboxModule } from 'primeng/checkbox';
import { CardModule } from 'primeng/card';
import { MessageService } from 'primeng/api';
import { ProductsService } from '../../core/services/products.service';
import { PageHeaderComponent } from '../../shared/components/page-header.component';
import { TranslateModule } from '@ngx-translate/core';
import { Producto } from '../../core/models/producto.model';

@Component({
  selector: 'app-productos-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    InputTextModule,
    TextareaModule,
    InputNumberModule,
    ButtonModule,
    CheckboxModule,
    CardModule,
    PageHeaderComponent,
    TranslateModule,
    RouterLink
  ],
  templateUrl: './productos-form.component.html'
})
export class ProductosFormComponent implements OnInit {
  readonly form;

  isEdit = false;
  productoId: string | null = null;

  constructor(
    private readonly fb: FormBuilder,
    private readonly products: ProductsService,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly messages: MessageService
  ) {
    this.form = this.fb.group({
      nombre: ['', Validators.required],
      sku: [''],
      descripcion: [''],
      precioBase: [0, [Validators.required, Validators.min(0)]],
      unidad: ['Unidad'],
      stockActual: [0, [Validators.required, Validators.min(0)]],
      stockMinimo: [0, [Validators.required, Validators.min(0)]],
      activo: [true]
    });
  }

  ngOnInit(): void {
    this.productoId = this.route.snapshot.paramMap.get('id');
    if (this.productoId) {
      const producto = this.products.getById(this.productoId);
      if (producto) {
        this.form.patchValue(producto);
        this.isEdit = true;
      }
    }
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const value = this.form.getRawValue() as Partial<Producto>;
    if (this.isEdit && this.productoId) {
      this.products.update(this.productoId, value);
      this.messages.add({ severity: 'info', summary: 'Actualizado', detail: 'Producto actualizado' });
    } else {
      this.products.create(value as any);
      this.messages.add({ severity: 'info', summary: 'Creado', detail: 'Producto creado' });
    }
    this.router.navigate(['/app/productos/lista']);
  }
}
