import { PermissionsMap } from './app-module.model';

export type RolUsuario = 'ADMIN' | 'CAJA' | 'BODEGA';

export interface Usuario {
  id: string;
  nombre: string;
  email: string;
  rol: RolUsuario;
  permisos?: PermissionsMap;
}
