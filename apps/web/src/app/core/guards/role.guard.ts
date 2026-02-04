import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { RolesService } from '../services/roles.service';
import { AuthService } from '../services/auth.service';
import { PermissionKey } from '../models/app-module.model';
import { RolUsuario } from '../models/usuario.model';
import { MessageService } from 'primeng/api';

export const roleGuard: CanActivateFn = (route) => {
  const auth = inject(AuthService);
  const rolesService = inject(RolesService);
  const router = inject(Router);
  const messages = inject(MessageService);
  const user = auth.snapshot;
  if (!user) {
    return router.createUrlTree(['/auth/login']);
  }

  const requiredRoles = route.data?.['roles'] as RolUsuario[] | undefined;
  if (requiredRoles && !requiredRoles.includes(user.rol)) {
    messages.add({
      severity: 'warn',
      summary: 'Sin permisos',
      detail: 'No tienes permisos para acceder a esta seccion.'
    });
    return router.createUrlTree(['/app/dashboard']);
  }

  const requiredPermissions = route.data?.['permissions'] as PermissionKey[] | undefined;
  if (requiredPermissions && requiredPermissions.length > 0) {
    const permisos = rolesService.resolvePermisos(user);
    const allowed = requiredPermissions.every((permiso) => permisos[permiso]);
    if (!allowed) {
      messages.add({
        severity: 'warn',
        summary: 'Sin permisos',
        detail: 'No tienes permisos para acceder a esta seccion.'
      });
      return router.createUrlTree(['/app/dashboard']);
    }
  }

  return true;
};
