export type AppModuleKey =
  | 'dashboard'
  | 'productos'
  | 'inventario'
  | 'ventas'
  | 'facturas'
  | 'cartera'
  | 'clientes'
  | 'admin'
  | 'faseSiguiente';

export type PermissionKey =
  | 'dashboard:view'
  | 'productos:view'
  | 'productos:create'
  | 'productos:edit'
  | 'productos:delete'
  | 'inventario:view'
  | 'inventario:entrada'
  | 'ventas:facturaNueva'
  | 'ventas:facturas'
  | 'ventas:verCartera'
  | 'ventas:marcarPagada'
  | 'clientes:view'
  | 'clientes:create'
  | 'clientes:edit'
  | 'clientes:delete'
  | 'admin:usuariosRoles'
  | 'faseSiguiente:view'
  | 'reportes:view';

export type PermissionsMap = Record<PermissionKey, boolean>;
