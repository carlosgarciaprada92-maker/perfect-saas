import { Component } from '@angular/core';
import { AsyncPipe, NgIf, NgFor } from '@angular/common';
import { TableModule } from 'primeng/table';
import { SelectModule } from 'primeng/select';
import { CheckboxModule } from 'primeng/checkbox';
import { ButtonModule } from 'primeng/button';
import { FormsModule } from '@angular/forms';
import { RolesService } from '../../core/services/roles.service';
import { Usuario, RolUsuario } from '../../core/models/usuario.model';
import { PermissionKey, PermissionsMap } from '../../core/models/app-module.model';
import { PageHeaderComponent } from '../../shared/components/page-header.component';
import { TranslateModule } from '@ngx-translate/core';
import { AuthService } from '../../core/services/auth.service';

interface PermissionItem {
  key: PermissionKey;
  labelKey: string;
}

interface PermissionGroup {
  labelKey: string;
  items: PermissionItem[];
}

@Component({
  selector: 'app-usuarios-roles',
  standalone: true,
  imports: [
    AsyncPipe,
    NgIf,
    NgFor,
    FormsModule,
    TableModule,
    SelectModule,
    CheckboxModule,
    ButtonModule,
    PageHeaderComponent,
    TranslateModule
  ],
  templateUrl: './usuarios-roles.component.html'
})
export class UsuariosRolesComponent {
  readonly usuarios$;
  selectedUser: Usuario | null = null;
  permisos: PermissionsMap = {} as PermissionsMap;

  readonly rolesOptions = [
    { label: 'Admin', value: 'ADMIN' },
    { label: 'Ventas', value: 'CAJA' },
    { label: 'Bodega', value: 'BODEGA' }
  ];

  readonly permissionGroups: PermissionGroup[] = [
    {
      labelKey: 'permisos.dashboard',
      items: [{ key: 'dashboard:view', labelKey: 'permisos.dashboardView' }]
    },
    {
      labelKey: 'permisos.productos',
      items: [
        { key: 'productos:view', labelKey: 'permisos.productosView' },
        { key: 'productos:create', labelKey: 'permisos.productosCreate' },
        { key: 'productos:edit', labelKey: 'permisos.productosEdit' },
        { key: 'productos:delete', labelKey: 'permisos.productosDelete' }
      ]
    },
    {
      labelKey: 'permisos.inventario',
      items: [
        { key: 'inventario:view', labelKey: 'permisos.inventarioView' },
        { key: 'inventario:entrada', labelKey: 'permisos.inventarioEntrada' }
      ]
    },
    {
      labelKey: 'permisos.ventas',
      items: [
        { key: 'ventas:facturaNueva', labelKey: 'permisos.ventasFacturaNueva' },
        { key: 'ventas:facturas', labelKey: 'permisos.ventasFacturas' },
        { key: 'ventas:verCartera', labelKey: 'permisos.ventasVerCartera' },
        { key: 'ventas:marcarPagada', labelKey: 'permisos.ventasMarcarPagada' }
      ]
    },
    {
      labelKey: 'permisos.clientes',
      items: [
        { key: 'clientes:view', labelKey: 'permisos.clientesView' },
        { key: 'clientes:create', labelKey: 'permisos.clientesCreate' },
        { key: 'clientes:edit', labelKey: 'permisos.clientesEdit' },
        { key: 'clientes:delete', labelKey: 'permisos.clientesDelete' }
      ]
    },
    {
      labelKey: 'permisos.admin',
      items: [{ key: 'admin:usuariosRoles', labelKey: 'permisos.adminUsuarios' }]
    },
    {
      labelKey: 'permisos.faseSiguiente',
      items: [{ key: 'faseSiguiente:view', labelKey: 'permisos.faseSiguienteView' }]
    },
    {
      labelKey: 'permisos.reportes',
      items: [{ key: 'reportes:view', labelKey: 'permisos.reportesView' }]
    }
  ];

  constructor(private readonly roles: RolesService, private readonly auth: AuthService) {
    this.usuarios$ = this.roles.usuarios$;
  }

  get canEdit(): boolean {
    return this.auth.snapshot?.rol === 'ADMIN';
  }

  roleLabel(rol: string): string {
    if (rol === 'CAJA') {
      return 'Ventas';
    }
    if (rol === 'ADMIN') {
      return 'Admin';
    }
    if (rol === 'BODEGA') {
      return 'Bodega';
    }
    return rol;
  }

  selectUser(user?: Usuario): void {
    if (!user) {
      return;
    }
    this.selectedUser = { ...user };
    this.permisos = { ...this.roles.resolvePermisos(user) };
  }

  handleRowSelect(event: any): void {
    this.selectUser(event?.data as Usuario | undefined);
  }

  onRoleChange(rol: RolUsuario): void {
    if (!this.selectedUser || !this.canEdit) {
      return;
    }
    this.selectedUser.rol = rol;
    this.permisos = { ...this.roles.getDefaultPermisos(rol) };
  }

  save(): void {
    if (!this.selectedUser || !this.canEdit) {
      return;
    }
    this.roles.updateUsuario(this.selectedUser.id, {
      rol: this.selectedUser.rol,
      permisos: { ...this.permisos }
    });
  }
}
