import { Component, DestroyRef, inject } from '@angular/core';
import { AsyncPipe, CurrencyPipe, DatePipe, NgIf, NgClass, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { SelectModule } from 'primeng/select';
import { AutoCompleteModule } from 'primeng/autocomplete';
import { CheckboxModule } from 'primeng/checkbox';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputNumberModule } from 'primeng/inputnumber';
import { BehaviorSubject } from 'rxjs';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ReportService, CreditRow, DashboardFilters, OverdueClienteRow } from '../../core/services/report.service';
import { CustomersService } from '../../core/services/customers.service';
import { InvoicesService } from '../../core/services/invoices.service';
import { RolesService } from '../../core/services/roles.service';
import { AuthService } from '../../core/services/auth.service';
import { PermissionKey } from '../../core/models/app-module.model';
import { Cliente } from '../../core/models/cliente.model';
import { ConfigService } from '../../core/services/config.service';
import { PageHeaderComponent } from '../../shared/components/page-header.component';
import { RouterLink } from '@angular/router';
import { DemoDataService } from '../../core/services/demo-data.service';
import { FiltersBarComponent } from '../../shared/components/filters-bar/filters-bar.component';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    AsyncPipe,
    CurrencyPipe,
    DatePipe,
    NgIf,
    NgClass,
    DecimalPipe,
    FormsModule,
    TableModule,
    TagModule,
    SelectModule,
    AutoCompleteModule,
    CheckboxModule,
    ButtonModule,
    DialogModule,
    InputNumberModule,
    TranslateModule,
    RouterLink,
    PageHeaderComponent,
    FiltersBarComponent
  ],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent {
  rangeOptions: Array<{ label: string; value: number }> = [];
  umbralOptions: Array<{ label: string; value: number }> = [];
  inactiveOptions: Array<{ label: string; value: number }> = [];

  rangeDays = 30;
  umbralDias = 5;
  inactiveDays = 30;
  includePagadas = false;

  clienteSeleccionado: Cliente | null = null;
  clienteSuggestions: Cliente[] = [];

  private readonly filtersSubject = new BehaviorSubject<DashboardFilters>({
    rangeDays: this.rangeDays,
    umbralDias: this.umbralDias,
    includePagadas: this.includePagadas,
    clienteId: null,
    inactiveDays: this.inactiveDays
  });

  readonly dashboard$;

  paymentVisible = false;
  paymentFactura: CreditRow | null = null;
  paymentAmount = 0;

  constructor(
    private readonly reportes: ReportService,
    private readonly customers: CustomersService,
    private readonly invoices: InvoicesService,
    private readonly roles: RolesService,
    private readonly auth: AuthService,
    private readonly config: ConfigService,
    private readonly translate: TranslateService,
    private readonly demoData: DemoDataService
  ) {
    const destroyRef = inject(DestroyRef);
    this.umbralDias = this.config.snapshot.umbralProximoVencer;
    this.filtersSubject.next({
      rangeDays: this.rangeDays,
      umbralDias: this.umbralDias,
      includePagadas: this.includePagadas,
      clienteId: null,
      inactiveDays: this.inactiveDays
    });
    this.dashboard$ = this.reportes.dashboard$(this.filtersSubject.asObservable());
    this.buildOptions();
    this.translate.onLangChange.pipe(takeUntilDestroyed(destroyRef)).subscribe(() => this.buildOptions());
  }

  updateFilters(): void {
    this.filtersSubject.next({
      rangeDays: this.rangeDays,
      umbralDias: this.umbralDias,
      includePagadas: this.includePagadas,
      clienteId: this.clienteSeleccionado?.id ?? null,
      inactiveDays: this.inactiveDays
    });
  }

  onSearchClientes(event: { query: string }): void {
    const term = event.query.toLowerCase();
    this.clienteSuggestions = this.customers.snapshot.filter((cliente) =>
      cliente.nombre.toLowerCase().includes(term)
    );
  }

  clearCliente(): void {
    this.clienteSeleccionado = null;
    this.updateFilters();
  }

  rowClass(row: CreditRow): string {
    if (row.estado === 'VENCIDO') {
      return 'row-overdue';
    }
    if (row.estado === 'POR_VENCER') {
      return 'row-soon';
    }
    return '';
  }

  openPayment(row: CreditRow): void {
    if (!this.can('ventas:marcarPagada')) {
      return;
    }
    this.paymentFactura = row;
    this.paymentAmount = row.factura.saldoPendiente ?? 0;
    this.paymentVisible = true;
  }

  confirmPayment(): void {
    if (!this.paymentFactura) {
      return;
    }
    this.invoices.registerPayment(this.paymentFactura.factura.id, this.paymentAmount);
    this.paymentVisible = false;
    this.paymentFactura = null;
    this.paymentAmount = 0;
  }

  markAsPaid(row: CreditRow): void {
    if (!this.can('ventas:marcarPagada')) {
      return;
    }
    this.invoices.markAsPaid(row.factura.id);
  }

  openPaymentForCliente(row: OverdueClienteRow): void {
    if (!row.facturaMasVencida) {
      return;
    }
    this.openPayment(row.facturaMasVencida);
  }

  filterByCliente(cliente?: Cliente): void {
    if (!cliente) {
      return;
    }
    this.clienteSeleccionado = cliente;
    this.updateFilters();
  }

  resetDemoData(): void {
    this.demoData.regenerate();
  }

  get isAdmin(): boolean {
    return this.auth.snapshot?.rol === 'ADMIN';
  }

  can(permission: PermissionKey): boolean {
    const user = this.auth.snapshot;
    if (!user) {
      return false;
    }
    return this.roles.hasPermission(user, permission);
  }

  private buildOptions(): void {
    const diasLabel = this.translate.instant('reportes.dias');
    this.rangeOptions = [7, 30, 90].map((value) => ({ value, label: `${value} ${diasLabel}` }));
    this.umbralOptions = [3, 5, 7].map((value) => ({ value, label: `${value} ${diasLabel}` }));
    this.inactiveOptions = [30, 60, 90].map((value) => ({ value, label: `${value} ${diasLabel}` }));
  }
}
