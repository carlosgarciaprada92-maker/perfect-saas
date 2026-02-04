export interface Producto {
  id: string;
  sku?: string;
  nombre: string;
  descripcion?: string;
  precioBase: number;
  unidad?: string;
  stockActual: number;
  stockMinimo: number;
  activo: boolean;
  createdAt: string;
}
