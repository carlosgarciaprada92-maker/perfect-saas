import { Component } from '@angular/core';
import { NgFor } from '@angular/common';
import { PageHeaderComponent } from '../../shared/components/page-header.component';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-fase-siguiente',
  standalone: true,
  imports: [NgFor, PageHeaderComponent, TranslateModule],
  templateUrl: './fase-siguiente.component.html'
})
export class FaseSiguienteComponent {
  readonly items = [
    'fase.whatsapp',
    'fase.dashboardFinanciero',
    'fase.reportes',
    'fase.pagosEmpleados',
    'fase.facturacionElectronica',
    'fase.appMovil'
  ];
}
