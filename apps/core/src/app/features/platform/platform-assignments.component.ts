import { Component, OnInit } from '@angular/core';
import { NgFor, NgIf } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { SelectModule } from 'primeng/select';
import { ButtonModule } from 'primeng/button';
import { ToggleButtonModule } from 'primeng/togglebutton';
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

  constructor(private readonly platform: PlatformService) {}

  ngOnInit(): void {
    this.platform.listTenants().subscribe((tenants) => {
      this.tenants = tenants;
      if (tenants.length > 0) {
        this.selectedTenantId = tenants[0].id;
        this.loadAssignments();
      } else {
        this.loading = false;
      }
    });
  }

  loadAssignments(): void {
    if (!this.selectedTenantId) {
      return;
    }
    this.loading = true;
    this.platform.listAssignments(this.selectedTenantId).subscribe({
      next: (assignments) => {
        this.assignments = assignments;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
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
}
