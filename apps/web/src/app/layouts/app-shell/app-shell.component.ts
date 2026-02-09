import { Component, computed, signal, DestroyRef, inject } from '@angular/core';
import { NgClass, NgFor, NgIf } from '@angular/common';
import { Router, RouterOutlet, RouterLink, RouterLinkActive, NavigationEnd } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { SelectModule } from 'primeng/select';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { FormsModule } from '@angular/forms';
import { TagModule } from 'primeng/tag';
import { AuthService } from '../../core/services/auth.service';
import { RolesService } from '../../core/services/roles.service';
import { PermissionKey } from '../../core/models/app-module.model';
import { RolUsuario } from '../../core/models/usuario.model';
import { filter } from 'rxjs/operators';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { environment } from '../../../environments/environment';

interface NavItem {
  labelKey: string;
  icon: string;
  route: string;
  permissions: PermissionKey[];
}

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [
    NgFor,
    NgIf,
    NgClass,
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    TranslateModule,
    SelectModule,
    ButtonModule,
    InputTextModule,
    FormsModule,
    TagModule
  ],
  templateUrl: './app-shell.component.html',
  styleUrl: './app-shell.component.scss'
})
export class AppShellComponent {
  readonly collapsed = signal(false);
  readonly sidebarOpen = signal(false);
  readonly isMobile = signal(false);
  searchTerm = '';
  readonly appName = environment.appName;

  readonly navItems: NavItem[] = [
    {
      labelKey: 'menu.dashboard',
      icon: 'pi pi-th-large',
      route: '/app/dashboard',
      permissions: ['dashboard:view']
    },
    {
      labelKey: 'menu.productos',
      icon: 'pi pi-box',
      route: '/app/productos/lista',
      permissions: ['productos:view']
    },
    {
      labelKey: 'menu.inventario',
      icon: 'pi pi-inbox',
      route: '/app/inventario/entrada',
      permissions: ['inventario:entrada']
    },
    {
      labelKey: 'menu.facturaNueva',
      icon: 'pi pi-receipt',
      route: '/app/ventas/factura-nueva',
      permissions: ['ventas:facturaNueva']
    },
    {
      labelKey: 'menu.facturas',
      icon: 'pi pi-file',
      route: '/app/ventas/facturas',
      permissions: ['ventas:facturas']
    },
    {
      labelKey: 'menu.cartera',
      icon: 'pi pi-wallet',
      route: '/app/cartera',
      permissions: ['ventas:verCartera']
    },
    {
      labelKey: 'menu.clientes',
      icon: 'pi pi-users',
      route: '/app/clientes',
      permissions: ['clientes:view']
    },
    {
      labelKey: 'menu.admin',
      icon: 'pi pi-shield',
      route: '/app/admin/usuarios-roles',
      permissions: ['admin:usuariosRoles']
    },
    {
      labelKey: 'menu.reportes',
      icon: 'pi pi-chart-line',
      route: '/app/reportes',
      permissions: ['reportes:view']
    },
    {
      labelKey: 'menu.faseSiguiente',
      icon: 'pi pi-sparkles',
      route: '/app/fase-siguiente',
      permissions: ['faseSiguiente:view']
    }
  ];

  readonly filteredNav = computed(() => {
    const user = this.auth.snapshot;
    if (!user) {
      return [];
    }
    const permisos = this.rolesService.resolvePermisos(user);
    return this.navItems.filter((item) =>
      item.permissions.every((permiso) => permisos[permiso])
    );
  });

  readonly idiomas = [
    { label: 'ES', value: 'es' },
    { label: 'EN', value: 'en' }
  ];

  constructor(
    public readonly auth: AuthService,
    private readonly rolesService: RolesService,
    public readonly translate: TranslateService,
    private readonly router: Router
  ) {
    const destroyRef = inject(DestroyRef);
    this.updateViewport();
    window.addEventListener('resize', this.updateViewport);

    this.router.events
      .pipe(filter((event) => event instanceof NavigationEnd), takeUntilDestroyed(destroyRef))
      .subscribe(() => {
        if (this.isMobile()) {
          this.sidebarOpen.set(false);
        }
      });

    destroyRef.onDestroy(() => {
      window.removeEventListener('resize', this.updateViewport);
    });
  }

  toggleSidebar(): void {
    if (this.isMobile()) {
      this.sidebarOpen.update((value) => !value);
      return;
    }
    this.collapsed.update((value) => !value);
  }

  toggleCollapse(): void {
    if (this.isMobile()) {
      this.sidebarOpen.update((value) => !value);
      return;
    }
    this.collapsed.update((value) => !value);
  }

  closeMobile(): void {
    this.sidebarOpen.set(false);
  }

  changeLanguage(lang: string): void {
    this.translate.use(lang);
    localStorage.setItem('inv_lang', lang);
  }

  roleLabel(rol?: RolUsuario): string {
    if (rol === 'ADMIN') {
      return 'Admin';
    }
    if (rol === 'CAJA') {
      return 'Ventas';
    }
    if (rol === 'BODEGA') {
      return 'Bodega';
    }
    return '';
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/auth/login']);
  }

  private updateViewport = (): void => {
    const mobile = window.innerWidth <= 1024;
    this.isMobile.set(mobile);
    if (!mobile) {
      this.sidebarOpen.set(false);
      return;
    }
    this.sidebarOpen.set(false);
    this.collapsed.set(false);
  };
}
