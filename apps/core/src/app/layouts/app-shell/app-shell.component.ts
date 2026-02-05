import { Component, OnInit } from '@angular/core';
import { NgIf, NgClass } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { TagModule } from 'primeng/tag';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from '../../core/services/auth.service';
import { PlatformService } from '../../core/services/platform.service';
import { TenantSummary } from '../../core/models/platform.model';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [
    NgIf,
    NgClass,
    FormsModule,
    RouterModule,
    TranslateModule,
    ButtonModule,
    SelectModule,
    TagModule,
    MatIconModule
  ],
  templateUrl: './app-shell.component.html',
  styleUrl: './app-shell.component.scss'
})
export class AppShellComponent implements OnInit {
  sidebarOpen = false;
  collapsed = false;
  tenantOptions: { label: string; value: string }[] = [];
  selectedTenant: string | null = null;

  constructor(
    private readonly auth: AuthService,
    private readonly platform: PlatformService,
    private readonly router: Router,
    public readonly translate: TranslateService
  ) {}

  ngOnInit(): void {
    if (this.isPlatformAdmin) {
      this.platform.listTenants().subscribe((tenants) => this.setupTenantOptions(tenants));
    }
  }

  get userName(): string {
    return this.auth.user?.name ?? 'Perfect';
  }

  get userEmail(): string {
    return this.auth.user?.email ?? '';
  }

  get tenantLabel(): string | null {
    return this.auth.tenantSlug ?? null;
  }

  get isPlatformAdmin(): boolean {
    return this.auth.hasRole('PlatformAdmin');
  }

  get isTenantAdmin(): boolean {
    return this.auth.hasRole('TenantAdmin') || this.auth.hasRole('ADMIN');
  }

  get showTenantSelector(): boolean {
    return this.isPlatformAdmin && this.tenantOptions.length > 0;
  }

  toggleSidebar(): void {
    this.sidebarOpen = !this.sidebarOpen;
  }

  closeSidebar(): void {
    this.sidebarOpen = false;
  }

  toggleCollapse(): void {
    this.collapsed = !this.collapsed;
  }

  switchLanguage(): void {
    const next = this.translate.currentLang === 'es' ? 'en' : 'es';
    this.translate.use(next);
    localStorage.setItem('core_lang', next);
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/auth/login']);
  }

  private setupTenantOptions(tenants: TenantSummary[]): void {
    this.tenantOptions = tenants.map((tenant) => ({
      label: tenant.displayName || tenant.name,
      value: tenant.slug
    }));
  }
}
