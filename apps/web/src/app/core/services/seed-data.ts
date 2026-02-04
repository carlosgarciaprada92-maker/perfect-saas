import { Cliente } from '../models/cliente.model';
import { Factura, FacturaItem, TipoPago } from '../models/factura.model';
import { MovimientoInventario } from '../models/movimiento-inventario.model';
import { Producto } from '../models/producto.model';
import { Usuario } from '../models/usuario.model';

const baseDate = new Date();
baseDate.setHours(0, 0, 0, 0);
const MS_DAY = 24 * 60 * 60 * 1000;

const toIso = (date: Date) => date.toISOString();
const daysFromBase = (offset: number) => toIso(new Date(baseDate.getTime() + offset * MS_DAY));
const daysAgo = (days: number) => daysFromBase(-days);
const addDays = (dateIso: string, days: number) => {
  const date = new Date(dateIso);
  date.setDate(date.getDate() + days);
  return date.toISOString();
};

const createRng = (seed: number) => {
  let value = seed % 2147483647;
  if (value <= 0) {
    value += 2147483646;
  }
  return () => {
    value = (value * 16807) % 2147483647;
    return (value - 1) / 2147483646;
  };
};

const randInt = (rng: () => number, min: number, max: number) =>
  Math.floor(rng() * (max - min + 1)) + min;

const pick = <T>(rng: () => number, list: T[]): T => list[randInt(rng, 0, list.length - 1)];

const baseProductos: Producto[] = [
  {
    id: 'prod-001',
    sku: 'ACE-001',
    nombre: 'Aceite 20W-50',
    descripcion: 'Aceite mineral para motores a gasolina.',
    precioBase: 48000,
    unidad: 'Litro',
    stockActual: 38,
    stockMinimo: 10,
    activo: true,
    createdAt: daysAgo(6)
  },
  {
    id: 'prod-002',
    sku: 'FILT-010',
    nombre: 'Filtro de aire',
    descripcion: 'Filtro estandar para vehiculos livianos.',
    precioBase: 22000,
    unidad: 'Unidad',
    stockActual: 12,
    stockMinimo: 8,
    activo: true,
    createdAt: daysAgo(18)
  },
  {
    id: 'prod-003',
    sku: 'BATT-120',
    nombre: 'Bateria 12V 60Ah',
    descripcion: 'Garantia 12 meses.',
    precioBase: 320000,
    unidad: 'Unidad',
    stockActual: 14,
    stockMinimo: 4,
    activo: true,
    createdAt: daysAgo(45)
  },
  {
    id: 'prod-004',
    sku: 'BUJ-IR',
    nombre: 'Bujia iridium',
    descripcion: 'Rendimiento premium.',
    precioBase: 35000,
    unidad: 'Unidad',
    stockActual: 46,
    stockMinimo: 12,
    activo: true,
    createdAt: daysAgo(8)
  },
  {
    id: 'prod-005',
    sku: 'DOT4-01',
    nombre: 'Liquido frenos DOT4',
    descripcion: 'Envase 500ml.',
    precioBase: 18000,
    unidad: 'Unidad',
    stockActual: 24,
    stockMinimo: 6,
    activo: true,
    createdAt: daysAgo(22)
  },
  {
    id: 'prod-006',
    sku: 'COR-ALT',
    nombre: 'Correa alternador',
    descripcion: 'Para motores 1.4-1.6.',
    precioBase: 52000,
    unidad: 'Unidad',
    stockActual: 15,
    stockMinimo: 6,
    activo: true,
    createdAt: daysAgo(3)
  },
  {
    id: 'prod-007',
    sku: 'PAST-DF',
    nombre: 'Pastillas freno delantera',
    descripcion: 'Juego completo.',
    precioBase: 145000,
    unidad: 'Juego',
    stockActual: 9,
    stockMinimo: 5,
    activo: true,
    createdAt: daysAgo(12)
  },
  {
    id: 'prod-008',
    sku: 'REF-AZ',
    nombre: 'Refrigerante azul',
    descripcion: 'Galon 3.8L.',
    precioBase: 42000,
    unidad: 'Galon',
    stockActual: 18,
    stockMinimo: 8,
    activo: true,
    createdAt: daysAgo(16)
  },
  {
    id: 'prod-009',
    sku: 'BOMB-H4',
    nombre: 'Bombilla H4',
    descripcion: 'Luz blanca 60/55W.',
    precioBase: 12000,
    unidad: 'Unidad',
    stockActual: 55,
    stockMinimo: 15,
    activo: true,
    createdAt: daysAgo(2)
  },
  {
    id: 'prod-010',
    sku: 'KIT-INJ',
    nombre: 'Kit limpieza inyectores',
    descripcion: 'Uso cada 10.000 km.',
    precioBase: 38000,
    unidad: 'Kit',
    stockActual: 16,
    stockMinimo: 6,
    activo: true,
    createdAt: daysAgo(27)
  },
  {
    id: 'prod-011',
    sku: 'ATF-01',
    nombre: 'Aceite caja ATF',
    descripcion: '1 litro.',
    precioBase: 62000,
    unidad: 'Litro',
    stockActual: 12,
    stockMinimo: 6,
    activo: true,
    createdAt: daysAgo(33)
  },
  {
    id: 'prod-012',
    sku: 'KIT-TOL',
    nombre: 'Kit herramientas basico',
    descripcion: 'Llaves, destornilladores y alicates.',
    precioBase: 98000,
    unidad: 'Kit',
    stockActual: 6,
    stockMinimo: 3,
    activo: true,
    createdAt: daysAgo(60)
  },
  {
    id: 'prod-013',
    sku: 'LUB-05',
    nombre: 'Lubricante multiproposito',
    descripcion: 'Aerosol 300ml.',
    precioBase: 16000,
    unidad: 'Unidad',
    stockActual: 28,
    stockMinimo: 8,
    activo: true,
    createdAt: daysAgo(10)
  },
  {
    id: 'prod-014',
    sku: 'ESCOB-01',
    nombre: 'Escobillas limpia parabrisas',
    descripcion: 'Par 16-18 pulgadas.',
    precioBase: 30000,
    unidad: 'Par',
    stockActual: 20,
    stockMinimo: 7,
    activo: true,
    createdAt: daysAgo(14)
  },
  {
    id: 'prod-015',
    sku: 'ANT-01',
    nombre: 'Anticongelante rojo',
    descripcion: 'Galon 3.8L.',
    precioBase: 45000,
    unidad: 'Galon',
    stockActual: 16,
    stockMinimo: 6,
    activo: true,
    createdAt: daysAgo(26)
  },
  {
    id: 'prod-016',
    sku: 'PAST-TR',
    nombre: 'Pastillas freno trasera',
    descripcion: 'Juego completo.',
    precioBase: 135000,
    unidad: 'Juego',
    stockActual: 8,
    stockMinimo: 4,
    activo: true,
    createdAt: daysAgo(20)
  }
];

const baseClientes: Cliente[] = [
  {
    id: 'cli-001',
    nombre: 'Transportes Andinos',
    identificacion: '900345001',
    telefono: '3105552211',
    email: 'compras@andinos.com',
    plazoCreditoDias: 15,
    activo: true,
    createdAt: daysAgo(40)
  },
  {
    id: 'cli-002',
    nombre: 'Distribuciones El Centro',
    identificacion: '800123992',
    telefono: '3205557788',
    email: 'finanzas@elcentro.co',
    plazoCreditoDias: 30,
    activo: true,
    createdAt: daysAgo(12)
  },
  {
    id: 'cli-003',
    nombre: 'Taller La 14',
    identificacion: '1122334455',
    telefono: '3002221100',
    email: 'taller14@gmail.com',
    plazoCreditoDias: 8,
    activo: true,
    createdAt: daysAgo(70)
  },
  {
    id: 'cli-004',
    nombre: 'Logistica Pacifico',
    identificacion: '901554200',
    telefono: '3115553399',
    email: 'compras@pacifico.com',
    plazoCreditoDias: 15,
    activo: true,
    createdAt: daysAgo(18)
  },
  {
    id: 'cli-005',
    nombre: 'Flota Norte',
    identificacion: '830204551',
    telefono: '3152006677',
    email: 'pagos@flotanorte.com',
    plazoCreditoDias: 30,
    activo: true,
    createdAt: daysAgo(55)
  },
  {
    id: 'cli-006',
    nombre: 'Servicios Rapidos',
    identificacion: '1044558899',
    telefono: '3008899000',
    email: 'admin@servrapidos.com',
    plazoCreditoDias: 8,
    activo: true,
    createdAt: daysAgo(90)
  },
  {
    id: 'cli-007',
    nombre: 'Repuestos Central',
    identificacion: '901220045',
    telefono: '3123332211',
    email: 'compras@repcentral.com',
    plazoCreditoDias: 15,
    activo: true,
    createdAt: daysAgo(25)
  },
  {
    id: 'cli-008',
    nombre: 'Taller El Sol',
    identificacion: '112333444',
    telefono: '3001115566',
    email: 'tallerelsol@gmail.com',
    plazoCreditoDias: 30,
    activo: true,
    createdAt: daysAgo(110)
  },
  {
    id: 'cli-009',
    nombre: 'Carga Express',
    identificacion: '901883112',
    telefono: '3134442211',
    email: 'compras@cargaexpress.com',
    plazoCreditoDias: 15,
    activo: true,
    createdAt: daysAgo(22)
  },
  {
    id: 'cli-010',
    nombre: 'Servicios del Sur',
    identificacion: '900221990',
    telefono: '3145556677',
    email: 'finanzas@servsur.com',
    plazoCreditoDias: 30,
    activo: true,
    createdAt: daysAgo(140)
  },
  {
    id: 'cli-011',
    nombre: 'Rutas Seguras',
    identificacion: '830998221',
    telefono: '3162201100',
    email: 'pagos@rutaseguras.com',
    plazoCreditoDias: 8,
    activo: true,
    createdAt: daysAgo(160)
  },
  {
    id: 'cli-012',
    nombre: 'Taller San Miguel',
    identificacion: '901119334',
    telefono: '3178003322',
    email: 'tallersanmiguel@gmail.com',
    plazoCreditoDias: 15,
    activo: true,
    createdAt: daysAgo(75)
  }
];

const buildItems = (rng: () => number, productos: Producto[]): FacturaItem[] => {
  const items: FacturaItem[] = [];
  const count = randInt(rng, 1, 3);
  const used = new Set<string>();
  for (let i = 0; i < count; i += 1) {
    let producto = pick(rng, productos);
    while (used.has(producto.id)) {
      producto = pick(rng, productos);
    }
    used.add(producto.id);
    const cantidad = randInt(rng, 1, 4);
    const priceFactor = 0.9 + rng() * 0.3;
    const precioUnitario = Math.round(producto.precioBase * priceFactor / 100) * 100;
    items.push({
      productoId: producto.id,
      nombreProducto: producto.nombre,
      cantidad,
      precioUnitario,
      totalLinea: precioUnitario * cantidad
    });
  }
  return items;
};

const buildFacturas = (seed: number, productos: Producto[], clientes: Cliente[]): Factura[] => {
  const rng = createRng(seed);
  const facturas: Factura[] = [];
  const creditClients = clientes.slice(0, 9);
  const count = 48;
  const creditPlazos = [8, 15, 30];
  const overduePlazos = [8, 15];

  for (let i = 0; i < count; i += 1) {
    const tipoPago: TipoPago = rng() < 0.65 ? 'CREDITO' : 'CONTADO';
    const cliente = tipoPago === 'CREDITO' || rng() < 0.35 ? pick(rng, creditClients) : undefined;
    let createdAt = daysAgo(randInt(rng, 0, 25));
    let fechaVencimiento: string | undefined;
    let plazoCreditoDiasUsado: number | undefined;

    if (tipoPago === 'CREDITO') {
      const bucket = rng();
      plazoCreditoDiasUsado = pick(rng, bucket < 0.3 ? overduePlazos : creditPlazos);
      let dueOffset = 0;
      if (bucket < 0.3) {
        dueOffset = -randInt(rng, 5, 15);
      } else if (bucket < 0.6) {
        dueOffset = randInt(rng, 1, 5);
      } else {
        dueOffset = randInt(rng, 10, 30);
      }
      fechaVencimiento = daysFromBase(dueOffset);
      createdAt = addDays(fechaVencimiento, -plazoCreditoDiasUsado);
    } else {
      const age = rng() < 0.2 ? randInt(rng, 60, 120) : randInt(rng, 0, 30);
      createdAt = daysAgo(age);
    }

    const items = buildItems(rng, productos);
    const subtotal = items.reduce((acc, line) => acc + line.totalLinea, 0);
    const total = subtotal;

    let pagada = tipoPago === 'CONTADO';
    let saldoPendiente = pagada ? 0 : total;
    let fechaPago: string | undefined = pagada ? createdAt : undefined;

    if (tipoPago === 'CREDITO') {
      const pagoRoll = rng();
      if (pagoRoll < 0.12) {
        pagada = true;
        saldoPendiente = 0;
        fechaPago = addDays(createdAt, randInt(rng, 2, 10));
      } else if (pagoRoll < 0.38) {
        saldoPendiente = Math.round(total * (0.3 + rng() * 0.4));
      }
    }

    facturas.push({
      id: `fac-${String(i + 1).padStart(3, '0')}`,
      consecutivo: `FAC-${String(i + 1).padStart(4, '0')}`,
      createdAt,
      clienteId: cliente?.id,
      items,
      subtotal,
      impuestos: 0,
      total,
      tipoPago,
      plazoCreditoDiasUsado,
      fechaVencimiento,
      pagada,
      saldoPendiente,
      fechaPago
    });
  }

  return facturas;
};

const buildMovimientos = (seed: number, productos: Producto[]): MovimientoInventario[] => {
  const rng = createRng(seed + 99);
  const movimientos: MovimientoInventario[] = [];
  const count = 14;
  for (let i = 0; i < count; i += 1) {
    const producto = pick(rng, productos);
    movimientos.push({
      id: `mov-${String(i + 1).padStart(3, '0')}`,
      productoId: producto.id,
      tipo: 'ENTRADA',
      cantidad: randInt(rng, 4, 20),
      motivo: rng() < 0.5 ? 'Ingreso proveedor' : 'Reposicion semanal',
      createdAt: daysAgo(randInt(rng, 3, 30))
    });
  }
  return movimientos;
};

export const DEFAULT_DEMO_SEED = 20260204;

export const buildSeedData = (seed: number = DEFAULT_DEMO_SEED) => {
  const productos = baseProductos.map((item) => ({ ...item }));
  const clientes = baseClientes.map((item) => ({ ...item }));
  const facturas = buildFacturas(seed, productos, clientes);
  const movimientos = buildMovimientos(seed, productos);
  const usuarios: Usuario[] = [
    {
      id: 'usr-001',
      nombre: 'Ana Admin',
      email: 'admin@perfect.demo',
      rol: 'ADMIN'
    },
    {
      id: 'usr-002',
      nombre: 'Valentina Ventas',
      email: 'ventas@perfect.demo',
      rol: 'CAJA'
    },
    {
      id: 'usr-003',
      nombre: 'Beatriz Bodega',
      email: 'bodega@perfect.demo',
      rol: 'BODEGA'
    }
  ];

  return { productos, clientes, facturas, movimientos, usuarios };
};

const DEFAULT_DATA = buildSeedData();

export const seedProductos: Producto[] = DEFAULT_DATA.productos;
export const seedClientes: Cliente[] = DEFAULT_DATA.clientes;
export const seedFacturas: Factura[] = DEFAULT_DATA.facturas;
export const seedMovimientos: MovimientoInventario[] = DEFAULT_DATA.movimientos;
export const seedUsuarios: Usuario[] = DEFAULT_DATA.usuarios;
