import { Component } from '@angular/core';
import { AsyncPipe, CurrencyPipe, DatePipe, NgIf } from '@angular/common';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { DatePickerModule } from 'primeng/datepicker';
import { TagModule } from 'primeng/tag';
import { FormsModule } from '@angular/forms';
import { CheckboxModule } from 'primeng/checkbox';
import { ButtonModule } from 'primeng/button';
import { combineLatest, BehaviorSubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { InvoicesService } from '../../core/services/invoices.service';
import { CustomersService } from '../../core/services/customers.service';
import { ConfigService } from '../../core/services/config.service';
import { PageHeaderComponent } from '../../shared/components/page-header.component';
import { TranslateModule } from '@ngx-translate/core';
import { Factura } from '../../core/models/factura.model';
import { RolesService } from '../../core/services/roles.service';
import { AuthService } from '../../core/services/auth.service';
import { PermissionKey } from '../../core/models/app-module.model';

interface CarteraFilters {
  estado?: string;
  clienteId?: string;
  rango?: Date[] | null;
  verPagadas?: boolean;
}

@Component({
  selector: 'app-cartera',
  standalone: true,
  imports: [
    AsyncPipe,
    CurrencyPipe,
    DatePipe,
    NgIf,
    FormsModule,
    SelectModule,
    CheckboxModule,
    TableModule,
    DatePickerModule,
    TagModule,
    ButtonModule,
    PageHeaderComponent,
    TranslateModule
  ],
  templateUrl: './cartera.component.html'
})
export class CarteraComponent {
  readonly clientes$;
  readonly config$;
  private readonly filtersSubject = new BehaviorSubject<CarteraFilters>({});
  readonly filters$ = this.filtersSubject.asObservable();
  rangeValue: Date[] | null = null;

  readonly cartera$;
  readonly filtered$;
  readonly totalPendiente$;
  verPagadas = false;

  constructor(
    private readonly invoices: InvoicesService,
    private readonly customers: CustomersService,
    private readonly config: ConfigService,
    private readonly roles: RolesService,
    private readonly auth: AuthService
  ) {
    this.clientes$ = this.customers.clientes$;
    this.config$ = this.config.config$;
    this.cartera$ = this.invoices.facturas$.pipe(
      map((facturas) => facturas.filter((f) => f.tipoPago === 'CREDITO'))
    );
    this.filtered$ = combineLatest([this.cartera$, this.filters$]).pipe(
      map(([facturas, filters]) => this.applyFilters(facturas, filters))
    );
    this.totalPendiente$ = this.filtered$.pipe(
      map((facturas) => facturas.reduce((acc, item) => acc + (item.saldoPendiente ?? 0), 0))
    );
    this.invoices.refreshEstados();
  }

  updateFilter(key: keyof CarteraFilters, value: any): void {
    this.filtersSubject.next({ ...this.filtersSubject.value, [key]: value });
  }

  calcCargoAdmin(total: number): number {
    return (total * this.config.snapshot.cargoAdminPct) / 100;
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

  getClienteNombre(id?: string): string {
    if (!id) {
      return '-';
    }
    return this.customers.getById(id)?.nombre ?? '-';
  }

  togglePagadas(value: boolean): void {
    this.verPagadas = value;
    this.updateFilter('verPagadas', value);
  }

  markAsPaid(factura: Factura): void {
    if (!this.can('ventas:marcarPagada')) {
      return;
    }
    if (factura.saldoPendiente <= 0) {
      return;
    }
    this.invoices.markAsPaid(factura.id);
  }

  can(permission: PermissionKey): boolean {
    const user = this.auth.snapshot;
    if (!user) {
      return false;
    }
    return this.roles.hasPermission(user, permission);
  }

  private applyFilters(facturas: Factura[], filters: CarteraFilters): Factura[] {
    return facturas.filter((factura) => {
      const saldoPendiente = factura.saldoPendiente ?? 0;
      if (!filters.verPagadas && saldoPendiente <= 0) {
        return false;
      }
      if (filters.estado && factura.estadoCartera !== filters.estado) {
        return false;
      }
      if (filters.clienteId && factura.clienteId !== filters.clienteId) {
        return false;
      }
      if (filters.rango && filters.rango.length === 2) {
        const [start, end] = filters.rango;
        const fecha = new Date(factura.createdAt).getTime();
        if (start && fecha < start.getTime()) {
          return false;
        }
        if (end && fecha > end.getTime()) {
          return false;
        }
      }
      return true;
    });
  }
}
