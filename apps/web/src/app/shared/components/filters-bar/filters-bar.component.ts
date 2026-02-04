import { Component, EventEmitter, Input, Output } from '@angular/core';
import { NgIf } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SelectModule } from 'primeng/select';
import { AutoCompleteModule } from 'primeng/autocomplete';
import { CheckboxModule } from 'primeng/checkbox';
import { ButtonModule } from 'primeng/button';
import { TranslateModule } from '@ngx-translate/core';
import { Cliente } from '../../../core/models/cliente.model';

@Component({
  selector: 'app-filters-bar',
  standalone: true,
  imports: [NgIf, FormsModule, SelectModule, AutoCompleteModule, CheckboxModule, ButtonModule, TranslateModule],
  templateUrl: './filters-bar.component.html',
  styleUrl: './filters-bar.component.scss'
})
export class FiltersBarComponent {
  @Input() titleKey = 'dashboard.filtrosTitle';
  @Input() subtitleKey = 'dashboard.filtrosSubtitle';
  @Input() defaultRangeDays = 30;
  @Input() defaultUmbralDias = 5;
  @Input() defaultInactiveDays = 30;

  @Input() rangeOptions: Array<{ label: string; value: number }> = [];
  @Input() umbralOptions: Array<{ label: string; value: number }> = [];
  @Input() inactiveOptions: Array<{ label: string; value: number }> = [];

  @Input() rangeDays = 30;
  @Output() rangeDaysChange = new EventEmitter<number>();

  @Input() umbralDias = 5;
  @Output() umbralDiasChange = new EventEmitter<number>();

  @Input() inactiveDays = 30;
  @Output() inactiveDaysChange = new EventEmitter<number>();

  @Input() includePagadas = false;
  @Output() includePagadasChange = new EventEmitter<boolean>();

  @Input() clienteSeleccionado: Cliente | null = null;
  @Output() clienteSeleccionadoChange = new EventEmitter<Cliente | null>();

  @Input() clienteSuggestions: Cliente[] = [];
  @Output() searchClientes = new EventEmitter<{ query: string }>();

  @Input() showAction = false;
  @Input() actionLabelKey = 'dashboard.resetDemo';
  @Input() actionTooltipKey = 'dashboard.resetDemoTooltip';
  @Input() actionIcon = 'pi pi-refresh';
  @Output() actionClick = new EventEmitter<void>();

  @Input() showChips = true;

  @Output() filtersChange = new EventEmitter<void>();

  onFiltersChange(): void {
    this.filtersChange.emit();
  }

  clearCliente(): void {
    this.clienteSeleccionado = null;
    this.clienteSeleccionadoChange.emit(null);
    this.filtersChange.emit();
  }

  selectCliente(event: any): void {
    const value = event?.value ?? event ?? null;
    this.clienteSeleccionado = value;
    this.clienteSeleccionadoChange.emit(value);
    this.filtersChange.emit();
  }

  get hasActiveFilters(): boolean {
    return (
      this.rangeDays !== this.defaultRangeDays ||
      this.umbralDias !== this.defaultUmbralDias ||
      this.inactiveDays !== this.defaultInactiveDays ||
      this.includePagadas ||
      !!this.clienteSeleccionado
    );
  }
}
