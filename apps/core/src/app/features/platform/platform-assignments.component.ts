import { Component, OnInit, ChangeDetectorRef, NgZone } from '@angular/core';
import { NgFor, NgIf } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { SelectModule } from 'primeng/select';
import { ButtonModule } from 'primeng/button';
import { ToggleButtonModule } from 'primeng/togglebutton';
import { MessageService } from 'primeng/api';
import { catchError, finalize, of } from 'rxjs';
import { PlatformService } from '../../core/services/platform.service';
import { ModuleAssignment, TenantSummary } from '../../core/models/platform.model';

@Component({
  selector: 'app-platform-assignments',
  standalone: true,
  imports: [NgFor, NgIf, FormsModule, TranslateModule, SelectModule, ButtonModule, ToggleButtonModule],
  templateUrl: './platform-assignments.component.html',
  styleUrl: './platform-assignments.component.scss'
})
export class PlatformAssignmentsComponent implements OnInit {
  tenants: TenantSummary[] = [];
  assignments: ModuleAssignment[] = [];
  selectedTenantId: string | null = null;
  loading = true;

  constructor(
    private readonly platform: PlatformService,
    private readonly messages: MessageService,
    private readonly translate: TranslateService,
    private readonly cdr: ChangeDetectorRef,
    private readonly zone: NgZone
  ) {}

  ngOnInit(): void {
    this.platform
      .listTenants()
      .pipe(
        catchError(() => {
          this.zone.run(() => {
            this.tenants = [];
            this.messages.add({
              severity: 'error',
              summary: this.translate.instant('common.error'),
              detail: this.translate.instant('platform.assignments.loadTenantsError')
            });
          });
          return of([] as TenantSummary[]);
        }),
        finalize(() => {
          this.zone.run(() => {
            if (!this.selectedTenantId) {
              this.loading = false;
            }
            this.cdr.markForCheck();
          });
        })
      )
      .subscribe((tenants) => {
        this.zone.run(() => {
          this.tenants = [...tenants];
          if (tenants.length > 0) {
            this.selectedTenantId = tenants[0].id;
            this.loadAssignments();
          }
        });
      });
  }

  loadAssignments(): void {
    if (!this.selectedTenantId) {
      return;
    }
    this.loading = true;
    this.platform
      .listAssignments(this.selectedTenantId)
      .pipe(
        catchError(() => {
          this.zone.run(() => {
            this.assignments = [];
            this.messages.add({
              severity: 'error',
              summary: this.translate.instant('common.error'),
              detail: this.translate.instant('platform.assignments.loadError')
            });
          });
          return of([] as ModuleAssignment[]);
        }),
        finalize(() => {
          this.zone.run(() => {
            this.loading = false;
            this.cdr.markForCheck();
          });
        })
      )
      .subscribe((assignments) => {
        this.zone.run(() => {
          this.assignments = [...assignments];
          this.cdr.markForCheck();
        });
      });
  }

  saveAssignments(): void {
    if (!this.selectedTenantId) {
      return;
    }
    this.platform
      .updateAssignments({
        tenantId: this.selectedTenantId,
        modules: this.assignments.map((module) => ({
          moduleId: module.moduleId,
          enabled: module.enabled
        }))
      })
      .subscribe();
  }

  copyUrl(module: ModuleAssignment): void {
    const url = (module.baseUrl ?? '').trim();
    if (!url) {
      this.messages.add({
        severity: 'warn',
        summary: this.translate.instant('common.notice'),
        detail: this.translate.instant('platform.modules.emptyUrl')
      });
      return;
    }

    navigator.clipboard
      .writeText(url)
      .then(() => {
        this.messages.add({
          severity: 'success',
          summary: this.translate.instant('common.notice'),
          detail: this.translate.instant('platform.assignments.copied')
        });
      })
      .catch(() => {
        this.messages.add({
          severity: 'error',
          summary: this.translate.instant('common.error'),
          detail: this.translate.instant('platform.assignments.copyError')
        });
      });
  }
}
