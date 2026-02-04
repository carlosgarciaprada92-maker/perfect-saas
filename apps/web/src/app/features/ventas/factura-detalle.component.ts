import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CurrencyPipe, DatePipe, NgIf, NgFor } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { InvoicesService } from '../../core/services/invoices.service';
import { CustomersService } from '../../core/services/customers.service';
import { Factura } from '../../core/models/factura.model';
import { TranslateModule } from '@ngx-translate/core';
import { PageHeaderComponent } from '../../shared/components/page-header.component';

@Component({
  selector: 'app-factura-detalle',
  standalone: true,
  imports: [NgIf, NgFor, CurrencyPipe, DatePipe, RouterLink, ButtonModule, TranslateModule, PageHeaderComponent],
  templateUrl: './factura-detalle.component.html'
})
export class FacturaDetalleComponent implements OnInit {
  factura: Factura | undefined;
  clienteNombre = '—';

  constructor(
    private readonly route: ActivatedRoute,
    private readonly invoices: InvoicesService,
    private readonly customers: CustomersService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.factura = this.invoices.getById(id);
      if (this.factura?.clienteId) {
        this.clienteNombre =
          this.customers.getById(this.factura.clienteId)?.nombre ?? this.clienteNombre;
      }
    }
  }

  imprimir(): void {
    window.print();
  }
}
