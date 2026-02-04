import { Component, DestroyRef, inject } from '@angular/core';
import { AsyncPipe, CurrencyPipe, DatePipe, NgIf, NgClass, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { SelectModule } from 'primeng/select';
import { AutoCompleteModule } from 'primeng/autocomplete';
import { CheckboxModule } from 'primeng/checkbox';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { DialogModule } from 'primeng/dialog';
import { TagModule } from 'primeng/tag';
import { TabsModule } from 'primeng/tabs';
import { BehaviorSubject } from 'rxjs';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ReportService, EstadoCredito, CreditRow } from '../../core/services/report.service';
import { CustomersService } from '../../core/services/customers.service';
import { InvoicesService } from '../../core/services/invoices.service';
import { RolesService } from '../../core/services/roles.service';
import { AuthService } from '../../core/services/auth.service';
import { PermissionKey } from '../../core/models/app-module.model';
import { Cliente } from '../../core/models/cliente.model';
import { PageHeaderComponent } from '../../shared/components/page-header.component';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

interface CarteraFilters {
  rangeDays: number;
  umbralDias: number;
  includePagadas: boolean;
  clienteId?: string | null;
  estado?: EstadoCredito | 'TODOS';
  search?: string;
}

@Component({
  selector: 'app-reportes',
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
    SelectModule,
    AutoCompleteModule,
    CheckboxModule,
    ButtonModule,
    InputTextModule,
    InputNumberModule,
    DialogModule,
    TagModule,
    TabsModule,
    TranslateModule,
    PageHeaderComponent
  ],
  templateUrl: './reportes.component.html'
})
export class ReportesComponent {
  rangeOptions: Array<{ label: string; value: number }> = [];
  umbralOptions: Array<{ label: string; value: number }> = [];
  readonly estadoOptions = [
    { label: 'TODOS', value: 'TODOS' },
    { label: 'AL_DIA', value: 'AL_DIA' },
    { label: 'POR_VENCER', value: 'POR_VENCER' },
    { label: 'VENCIDO', value: 'VENCIDO' },
    { label: 'PAGADA', value: 'PAGADA' }
  ];

  rangeDays = 30;
  umbralDias = 5;
  includePagadas = false;
  estado: EstadoCredito | 'TODOS' = 'TODOS';
  searchTerm = '';
  activeTab = 'cartera';

  clienteSeleccionado: Cliente | null = null;
  clienteSuggestions: Cliente[] = [];

  private readonly carteraFiltersSubject = new BehaviorSubject<CarteraFilters>({
    rangeDays: this.rangeDays,
    umbralDias: this.umbralDias,
    includePagadas: this.includePagadas,
    estado: this.estado,
    search: this.searchTerm,
    clienteId: null
  });
  readonly cartera$;

  private readonly ventasFiltersSubject = new BehaviorSubject({ rangeDays: this.rangeDays });
  readonly ventas$;

  private readonly clientesFiltersSubject = new BehaviorSubject({ rangeDays: this.rangeDays, inactiveDays: 30 });
  readonly clientes$;

  paymentVisible = false;
  paymentFactura: CreditRow | null = null;
  paymentAmount = 0;

  constructor(
    private readonly reportes: ReportService,
    private readonly customers: CustomersService,
    private readonly invoices: InvoicesService,
    private readonly roles: RolesService,
    private readonly auth: AuthService,
    private readonly translate: TranslateService
  ) {
    const destroyRef = inject(DestroyRef);
    this.cartera$ = this.reportes.carteraReport$(this.carteraFiltersSubject.asObservable());
    this.ventas$ = this.reportes.ventasReport$(this.ventasFiltersSubject.asObservable());
    this.clientes$ = this.reportes.clientesReport$(this.clientesFiltersSubject.asObservable());
    this.buildOptions();
    this.translate.onLangChange.pipe(takeUntilDestroyed(destroyRef)).subscribe(() => this.buildOptions());
  }

  updateFilters(): void {
    this.carteraFiltersSubject.next({
      rangeDays: this.rangeDays,
      umbralDias: this.umbralDias,
      includePagadas: this.includePagadas,
      estado: this.estado,
      search: this.searchTerm,
      clienteId: this.clienteSeleccionado?.id ?? null
    });
    this.ventasFiltersSubject.next({ rangeDays: this.rangeDays });
    this.clientesFiltersSubject.next({ rangeDays: this.rangeDays, inactiveDays: 30 });
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

  can(permission: PermissionKey): boolean {
    const user = this.auth.snapshot;
    if (!user) {
      return false;
    }
    return this.roles.hasPermission(user, permission);
  }

  exportCsv(rows: CreditRow[]): void {
    const headers = ['Cliente', 'Factura', 'Estado', 'Vencimiento', 'SaldoPendiente', 'Total'];
    const csvRows = rows.map((row) => [
      row.cliente?.nombre ?? '',
      row.factura.consecutivo,
      row.estado,
      row.factura.fechaVencimiento ?? '',
      String(row.factura.saldoPendiente ?? 0),
      String(row.factura.total)
    ]);
    const csv = [headers, ...csvRows]
      .map((line) => line.map((value) => `"${String(value).replace(/"/g, '""')}"`).join(','))
      .join('\n');

    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `reporte-cartera-${new Date().toISOString().slice(0, 10)}.csv`;
    link.click();
    URL.revokeObjectURL(url);
  }

  private buildOptions(): void {
    const diasLabel = this.translate.instant('reportes.dias');
    this.rangeOptions = [7, 30, 90].map((value) => ({ value, label: `${value} ${diasLabel}` }));
    this.umbralOptions = [3, 5, 7].map((value) => ({ value, label: `${value} ${diasLabel}` }));
  }
}
