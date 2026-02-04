import { Component } from '@angular/core';
import { AsyncPipe, CurrencyPipe, NgIf } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import { BadgeModule } from 'primeng/badge';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { SkeletonModule } from 'primeng/skeleton';
import { ConfirmationService, MessageService } from 'primeng/api';
import { debounceTime, map, startWith } from 'rxjs/operators';
import { combineLatest } from 'rxjs';
import { ProductsService } from '../../core/services/products.service';
import { ConfigService } from '../../core/services/config.service';
import { PageHeaderComponent } from '../../shared/components/page-header.component';
import { TranslateModule } from '@ngx-translate/core';
import { RolesService } from '../../core/services/roles.service';
import { AuthService } from '../../core/services/auth.service';
import { PermissionKey } from '../../core/models/app-module.model';

@Component({
  selector: 'app-productos-lista',
  standalone: true,
  imports: [
    AsyncPipe,
    CurrencyPipe,
    NgIf,
    RouterLink,
    ReactiveFormsModule,
    TableModule,
    ButtonModule,
    InputTextModule,
    TagModule,
    BadgeModule,
    ConfirmDialogModule,
    SkeletonModule,
    PageHeaderComponent,
    TranslateModule
  ],
  templateUrl: './productos-lista.component.html'
})
export class ProductosListaComponent {
  readonly productos$;
  readonly config$;
  readonly loading$;
  readonly filtered$;
  readonly searchControl = new FormControl('', { nonNullable: true });

  constructor(
    private readonly products: ProductsService,
    private readonly config: ConfigService,
    private readonly confirm: ConfirmationService,
    private readonly messages: MessageService,
    private readonly router: Router,
    private readonly roles: RolesService,
    private readonly auth: AuthService
  ) {
    this.productos$ = this.products.productos$;
    this.config$ = this.config.config$;
    this.loading$ = this.productos$.pipe(
      map(() => false),
      startWith(true)
    );
    const search$ = this.searchControl.valueChanges.pipe(
      startWith(''),
      debounceTime(200),
      map((value) => value.trim().toLowerCase())
    );
    this.filtered$ = combineLatest([this.productos$, search$]).pipe(
      map(([productos, term]) => {
        if (!term) {
          return productos;
        }
        return productos.filter((producto) => {
          const nombre = producto.nombre.toLowerCase();
          const sku = producto.sku?.toLowerCase() ?? '';
          const descripcion = producto.descripcion?.toLowerCase() ?? '';
          return (
            nombre.includes(term) ||
            sku.includes(term) ||
            descripcion.includes(term)
          );
        });
      })
    );
  }

  isNuevo(createdAt: string, diasNuevo: number): boolean {
    const created = new Date(createdAt).getTime();
    const diffDays = (Date.now() - created) / (24 * 60 * 60 * 1000);
    return diffDays <= diasNuevo;
  }

  deleteProducto(id: string): void {
    if (!this.can('productos:delete')) {
      return;
    }
    this.confirm.confirm({
      message: 'Eliminar producto?',
      header: 'Confirmacion',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.products.remove(id);
        this.messages.add({
          severity: 'info',
          summary: 'Eliminado',
          detail: 'Producto eliminado'
        });
      }
    });
  }

  goToEdit(id: string): void {
    if (!this.can('productos:edit')) {
      return;
    }
    this.router.navigate(['/app/productos', id, 'editar']);
  }

  can(permission: PermissionKey): boolean {
    const user = this.auth.snapshot;
    if (!user) {
      return false;
    }
    return this.roles.hasPermission(user, permission);
  }
}

