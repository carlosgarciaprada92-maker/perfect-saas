import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { TableModule } from 'primeng/table';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { PlatformService } from '../../core/services/platform.service';
import { TenantSummary } from '../../core/models/platform.model';

@Component({
  selector: 'app-platform-tenants',
  standalone: true,
  imports: [
    FormsModule,
    TranslateModule,
    TableModule,
    InputTextModule,
    TagModule,
    ButtonModule
  ],
  templateUrl: './platform-tenants.component.html',
  styleUrl: './platform-tenants.component.scss'
})
export class PlatformTenantsComponent implements OnInit {
  tenants: TenantSummary[] = [];
  search = '';
  loading = true;

  constructor(private readonly platform: PlatformService) {}

  ngOnInit(): void {
    this.loadTenants();
  }

  loadTenants(): void {
    this.loading = true;
    this.platform.listTenants(this.search).subscribe({
      next: (tenants) => {
        this.tenants = tenants;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  toggleStatus(tenant: TenantSummary): void {
    const next = tenant.status === 'Active' ? 'Suspended' : 'Active';
    this.platform.updateTenantStatus(tenant.id, { status: next }).subscribe((updated) => {
      tenant.status = updated.status;
    });
  }
}
