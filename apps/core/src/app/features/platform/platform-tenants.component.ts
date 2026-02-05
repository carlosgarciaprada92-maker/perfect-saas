import { Component, OnInit, ChangeDetectorRef, NgZone } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { TableModule } from 'primeng/table';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { MessageService } from 'primeng/api';
import { catchError, finalize, of } from 'rxjs';
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

  constructor(
    private readonly platform: PlatformService,
    private readonly messages: MessageService,
    private readonly translate: TranslateService,
    private readonly cdr: ChangeDetectorRef,
    private readonly zone: NgZone
  ) {}

  ngOnInit(): void {
    this.loadTenants();
  }

  loadTenants(): void {
    this.loading = true;
    this.platform
      .listTenants(this.search)
      .pipe(
        catchError(() => {
          this.zone.run(() => {
            this.tenants = [];
            this.messages.add({
              severity: 'error',
              summary: this.translate.instant('common.error'),
              detail: this.translate.instant('platform.tenants.loadError')
            });
          });
          return of([] as TenantSummary[]);
        }),
        finalize(() => {
          this.zone.run(() => {
            this.loading = false;
            this.cdr.markForCheck();
          });
        })
      )
      .subscribe((tenants) => {
        this.zone.run(() => {
          this.tenants = [...tenants];
          this.cdr.markForCheck();
        });
      });
  }

  toggleStatus(tenant: TenantSummary): void {
    const next = tenant.status === 'Active' ? 'Suspended' : 'Active';
    this.platform.updateTenantStatus(tenant.id, { status: next }).subscribe((updated) => {
      this.tenants = this.tenants.map((item) =>
        item.id === tenant.id ? { ...item, status: updated.status } : item
      );
      this.cdr.markForCheck();
    });
  }
}
