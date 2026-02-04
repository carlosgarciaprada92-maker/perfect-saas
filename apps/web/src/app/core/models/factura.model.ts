export interface FacturaItem {
  productoId: string;
  nombreProducto: string;
  cantidad: number;
  precioUnitario: number;
  totalLinea: number;
}

export type TipoPago = 'CONTADO' | 'CREDITO';
export type EstadoCartera = 'AL_DIA' | 'PROXIMO_VENCER' | 'VENCIDO' | 'PAGADA';

export interface Factura {
  id: string;
  consecutivo: string;
  createdAt: string;
  clienteId?: string;
  items: FacturaItem[];
  subtotal: number;
  impuestos?: number;
  total: number;
  tipoPago: TipoPago;
  plazoCreditoDiasUsado?: number;
  fechaVencimiento?: string;
  estadoCartera?: EstadoCartera;
  pagada: boolean;
  saldoPendiente: number;
  fechaPago?: string;
}
