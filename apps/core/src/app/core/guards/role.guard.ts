import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const roleGuard: CanActivateFn = (route) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  const roles = (route.data?.['roles'] as string[] | undefined) ?? [];
  if (roles.length === 0) {
    return true;
  }

  const matches = roles.some((role: string) => auth.hasRole(role));
  if (matches) {
    return true;
  }

  return router.parseUrl('/workspace');
};
