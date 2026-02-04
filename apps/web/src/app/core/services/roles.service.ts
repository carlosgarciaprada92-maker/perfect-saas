import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { PermissionKey, PermissionsMap } from '../models/app-module.model';
import { RolUsuario, Usuario } from '../models/usuario.model';
import { StorageService } from './storage.service';
import { seedUsuarios } from './seed-data';

const DEFAULT_PERMISSIONS: Record<RolUsuario, PermissionsMap> = {
  ADMIN: {
    'dashboard:view': true,
    'productos:view': true,
    'productos:create': true,
    'productos:edit': true,
    'productos:delete': true,
    'inventario:view': true,
    'inventario:entrada': true,
    'ventas:facturaNueva': true,
    'ventas:facturas': true,
    'ventas:verCartera': true,
    'ventas:marcarPagada': true,
    'clientes:view': true,
    'clientes:create': true,
    'clientes:edit': true,
    'clientes:delete': true,
    'admin:usuariosRoles': true,
    'faseSiguiente:view': true,
    'reportes:view': true
  },
  CAJA: {
    'dashboard:view': true,
    'productos:view': false,
    'productos:create': false,
    'productos:edit': false,
    'productos:delete': false,
    'inventario:view': false,
    'inventario:entrada': false,
    'ventas:facturaNueva': true,
    'ventas:facturas': true,
    'ventas:verCartera': true,
    'ventas:marcarPagada': true,
    'clientes:view': true,
    'clientes:create': true,
    'clientes:edit': true,
    'clientes:delete': false,
    'admin:usuariosRoles': false,
    'faseSiguiente:view': false,
    'reportes:view': true
  },
  BODEGA: {
    'dashboard:view': true,
    'productos:view': true,
    'productos:create': true,
    'productos:edit': true,
    'productos:delete': true,
    'inventario:view': true,
    'inventario:entrada': true,
    'ventas:facturaNueva': false,
    'ventas:facturas': false,
    'ventas:verCartera': false,
    'ventas:marcarPagada': false,
    'clientes:view': false,
    'clientes:create': false,
    'clientes:edit': false,
    'clientes:delete': false,
    'admin:usuariosRoles': false,
    'faseSiguiente:view': false,
    'reportes:view': false
  }
};

@Injectable({ providedIn: 'root' })
export class RolesService {
  private readonly storageKey = 'inv_usuarios';
  private readonly usersSubject = new BehaviorSubject<Usuario[]>(seedUsuarios);

  readonly usuarios$ = this.usersSubject.asObservable();

  constructor(private readonly storage: StorageService) {
    const stored = this.storage.get(this.storageKey, seedUsuarios);
    const normalized = stored.every((user) => !!user.email) ? stored : seedUsuarios;
    this.usersSubject.next(normalized);
    this.persist();
  }

  get snapshot(): Usuario[] {
    return this.usersSubject.value;
  }

  getDefaultPermisos(rol: RolUsuario): PermissionsMap {
    return { ...DEFAULT_PERMISSIONS[rol] };
  }

  resolvePermisos(usuario: Usuario): PermissionsMap {
    return { ...DEFAULT_PERMISSIONS[usuario.rol], ...(usuario.permisos ?? {}) };
  }

  hasPermission(usuario: Usuario, permission: PermissionKey): boolean {
    return this.resolvePermisos(usuario)[permission] === true;
  }

  updateUsuario(id: string, changes: Partial<Usuario>): void {
    const updated = this.snapshot.map((user) =>
      user.id === id ? { ...user, ...changes } : user
    );
    this.usersSubject.next(updated);
    this.persist();
  }

  upsertUsuario(usuario: Usuario): void {
    const exists = this.snapshot.find((u) => u.id === usuario.id);
    if (exists) {
      this.updateUsuario(usuario.id, usuario);
      return;
    }
    const updated = [...this.snapshot, usuario];
    this.usersSubject.next(updated);
    this.persist();
  }

  findByEmail(email: string): Usuario | undefined {
    return this.snapshot.find((user) => user.email.toLowerCase() === email.toLowerCase());
  }

  private persist(): void {
    this.storage.set(this.storageKey, this.snapshot);
  }
}
