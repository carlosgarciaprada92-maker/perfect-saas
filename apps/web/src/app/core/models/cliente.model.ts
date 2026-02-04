export interface Cliente {
  id: string;
  nombre: string;
  identificacion?: string;
  telefono?: string;
  email?: string;
  plazoCreditoDias: number;
  activo: boolean;
  createdAt?: string;
}
