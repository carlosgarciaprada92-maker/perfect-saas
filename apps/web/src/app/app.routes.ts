import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';
import { AppShellComponent } from './layouts/app-shell/app-shell.component';
import { LoginComponent } from './features/auth/login.component';

export const routes: Routes = [
  { path: '', redirectTo: 'auth/login', pathMatch: 'full' },
  {
    path: 'auth',
    children: [{ path: 'login', component: LoginComponent }]
  },
  {
    path: 'app',
    component: AppShellComponent,
    canActivate: [authGuard],
    children: [
      {
        path: 'dashboard',
        canActivate: [roleGuard],
        data: { permissions: ['dashboard:view'] },
        loadComponent: () => import('./features/dashboard/dashboard.component').then((m) => m.DashboardComponent)
      },
      {
        path: 'productos',
        children: [
          {
            path: 'lista',
            canActivate: [roleGuard],
            data: { permissions: ['productos:view'] },
            loadComponent: () =>
              import('./features/productos/productos-lista.component').then((m) => m.ProductosListaComponent)
          },
          {
            path: 'nuevo',
            canActivate: [roleGuard],
            data: { permissions: ['productos:create'] },
            loadComponent: () =>
              import('./features/productos/productos-form.component').then((m) => m.ProductosFormComponent)
          },
          {
            path: ':id/editar',
            canActivate: [roleGuard],
            data: { permissions: ['productos:edit'] },
            loadComponent: () =>
              import('./features/productos/productos-form.component').then((m) => m.ProductosFormComponent)
          },
          { path: '', redirectTo: 'lista', pathMatch: 'full' }
        ]
      },
      {
        path: 'inventario/entrada',
        canActivate: [roleGuard],
        data: { permissions: ['inventario:entrada'] },
        loadComponent: () =>
          import('./features/inventario/inventario-entrada.component').then(
            (m) => m.InventarioEntradaComponent
          )
      },
      {
        path: 'ventas/factura-nueva',
        canActivate: [roleGuard],
        data: { permissions: ['ventas:facturaNueva'] },
        loadComponent: () =>
          import('./features/ventas/factura-nueva.component').then((m) => m.FacturaNuevaComponent)
      },
      {
        path: 'ventas/facturas',
        canActivate: [roleGuard],
        data: { permissions: ['ventas:facturas'] },
        loadComponent: () =>
          import('./features/ventas/facturas-lista.component').then((m) => m.FacturasListaComponent)
      },
      {
        path: 'ventas/facturas/:id',
        canActivate: [roleGuard],
        data: { permissions: ['ventas:facturas'] },
        loadComponent: () =>
          import('./features/ventas/factura-detalle.component').then((m) => m.FacturaDetalleComponent)
      },
      {
        path: 'cartera',
        canActivate: [roleGuard],
        data: { permissions: ['ventas:verCartera'] },
        loadComponent: () => import('./features/cartera/cartera.component').then((m) => m.CarteraComponent)
      },
      {
        path: 'clientes',
        canActivate: [roleGuard],
        data: { permissions: ['clientes:view'] },
        loadComponent: () => import('./features/clientes/clientes.component').then((m) => m.ClientesComponent)
      },
      {
        path: 'admin/usuarios-roles',
        canActivate: [roleGuard],
        data: { permissions: ['admin:usuariosRoles'] },
        loadComponent: () =>
          import('./features/admin/usuarios-roles.component').then((m) => m.UsuariosRolesComponent)
      },
      {
        path: 'fase-siguiente',
        canActivate: [roleGuard],
        data: { permissions: ['faseSiguiente:view'] },
        loadComponent: () =>
          import('./features/fase-siguiente/fase-siguiente.component').then((m) => m.FaseSiguienteComponent)
      },
      {
        path: 'reportes',
        canActivate: [roleGuard],
        data: { permissions: ['reportes:view'] },
        loadComponent: () =>
          import('./features/reportes/reportes.component').then((m) => m.ReportesComponent)
      },
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
    ]
  },
  { path: '**', redirectTo: 'auth/login' }
];
