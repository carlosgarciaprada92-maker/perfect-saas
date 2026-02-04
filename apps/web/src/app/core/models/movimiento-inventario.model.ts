export type TipoMovimiento = 'ENTRADA';

export interface MovimientoInventario {
  id: string;
  productoId: string;
  tipo: TipoMovimiento;
  cantidad: number;
  motivo?: string;
  createdAt: string;
}
