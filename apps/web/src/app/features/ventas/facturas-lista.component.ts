import { Component } from '@angular/core';
import { AsyncPipe, CurrencyPipe, DatePipe, NgIf } from '@angular/common';
import { RouterLink } from '@angular/router';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { InvoicesService } from '../../core/services/invoices.service';
import { CustomersService } from '../../core/services/customers.service';
import { TranslateModule } from '@ngx-translate/core';
import { PageHeaderComponent } from '../../shared/components/page-header.component';
import { RolesService } from '../../core/services/roles.service';
import { AuthService } from '../../core/services/auth.service';
import { PermissionKey } from '../../core/models/app-module.model';

@Component({
  selector: 'app-facturas-lista',
  standalone: true,
  imports: [
    AsyncPipe,
    CurrencyPipe,
    DatePipe,
    NgIf,
    RouterLink,
    TableModule,
    TagModule,
    ButtonModule,
    TranslateModule,
    PageHeaderComponent
  ],
  templateUrl: './facturas-lista.component.html'
})
export class FacturasListaComponent {
  readonly facturas$;
  readonly clientes$;

  constructor(
    private readonly invoices: InvoicesService,
    private readonly customers: CustomersService,
    private readonly roles: RolesService,
    private readonly auth: AuthService
  ) {
    this.facturas$ = this.invoices.facturas$;
    this.clientes$ = this.customers.clientes$;
    this.invoices.refreshEstados();
  }

  getClienteNombre(id?: string, clientes = this.customers.snapshot): string {
    if (!id) {
      return '—';
    }
    return clientes.find((c) => c.id === id)?.nombre ?? '—';
  }

  statusSeverity(estado?: string): 'info' | 'warn' | 'danger' {
    if (estado === 'PAGADA') {
      return 'info';
    }
    if (estado === 'PROXIMO_VENCER') {
      return 'warn';
    }
    if (estado === 'VENCIDO') {
      return 'danger';
    }
    return 'info';
  }

  can(permission: PermissionKey): boolean {
    const user = this.auth.snapshot;
    if (!user) {
      return false;
    }
    return this.roles.hasPermission(user, permission);
  }
}
