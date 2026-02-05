import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';
import { AppShellComponent } from './layouts/app-shell/app-shell.component';
import { LoginComponent } from './features/auth/login.component';

export const routes: Routes = [
  { path: '', redirectTo: 'auth/login', pathMatch: 'full' },
  {
    path: 'auth',
    children: [{ path: 'login', component: LoginComponent, data: { mode: 'platform' } }]
  },
  {
    path: 'portal',
    children: [{ path: 'login', component: LoginComponent, data: { mode: 'portal' } }]
  },
  {
    path: '',
    component: AppShellComponent,
    canActivate: [authGuard],
    children: [
      {
        path: 'workspace',
        canActivate: [roleGuard],
        data: { roles: ['TenantAdmin', 'ADMIN'] },
        loadComponent: () =>
          import('./features/workspace/workspace-home.component').then((m) => m.WorkspaceHomeComponent)
      },
      {
        path: 'platform',
        canActivate: [roleGuard],
        data: { roles: ['PlatformAdmin'] },
        children: [
          {
            path: 'tenants',
            loadComponent: () =>
              import('./features/platform/platform-tenants.component').then((m) => m.PlatformTenantsComponent)
          },
          {
            path: 'modules',
            loadComponent: () =>
              import('./features/platform/platform-modules.component').then((m) => m.PlatformModulesComponent)
          },
          {
            path: 'assignments',
            loadComponent: () =>
              import('./features/platform/platform-assignments.component').then(
                (m) => m.PlatformAssignmentsComponent
              )
          },
          { path: '', redirectTo: 'tenants', pathMatch: 'full' }
        ]
      },
      { path: '', redirectTo: 'workspace', pathMatch: 'full' }
    ]
  },
  { path: '**', redirectTo: 'auth/login' }
];
