# Perfect Core

## Arquitectura
- Frontend: Angular 21 + PrimeNG + Angular Material (solo free), i18n ES/EN, PWA.
- Backend: .NET 8 Clean Architecture (Perfect.Domain / Perfect.Application / Perfect.Infrastructure / Perfect.Api).
- Multi-tenant: se mantiene el middleware actual (header/slug/claim) sin cambios.
- Infra: Docker + ECS/ECR + NGINX reverse proxy.

## Cómo correr local

### Backend
```bash
cd apps/api
# Inicia Postgres + API
docker compose up -d --build

# Migraciones + seed demo
curl -X POST http://localhost:8080/api/v1/auth/seed-demo
```

### Frontend Core
```bash
cd apps/core
npm ci
npm run start
```

## Variables de entorno

### apps/core
- `PERFECT_API_BASE_URL` (runtime global) o `src/environments/environment.ts`

### apps/api
- `ConnectionStrings__Default`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Jwt__Key`
- `Tenant__Mode`
- `Tenant__HeaderName`
- `Tenant__SlugHeaderName`
- `Cors__AllowedOrigins`

## Rutas principales

### Core (frontend)
- `/core/auth/login`
- `/core/workspace`
- `/core/platform/tenants`
- `/core/platform/modules`
- `/core/platform/assignments`

### API
- `GET /api/v1/platform/modules`
- `POST /api/v1/platform/modules`
- `PUT /api/v1/platform/modules/{id}`
- `DELETE /api/v1/platform/modules/{id}`
- `GET /api/v1/platform/tenants`
- `PUT /api/v1/platform/tenants/{id}/status`
- `GET /api/v1/platform/assignments?tenantId={id}`
- `PUT /api/v1/platform/assignments`
- `GET /api/v1/workspace/apps`
- `GET /api/v1/workspace/users` (stub)

## URLs DEV (actualizadas)
- `http://3.145.90.129/`
- `http://3.145.90.129/core/auth/login`
- `http://3.145.90.129/core/workspace`
- `http://3.145.90.129/core/platform/tenants`
- `http://3.145.90.129/api/health`
- `http://3.145.90.129/swagger/`

## Credenciales demo
- PlatformAdmin: `platform.admin@perfect.demo` / `Platform123!` (tenant: `platform`)
- TenantAdmin: `admin@perfect.demo` / `Admin123!` (tenant: `demo`)

## Smoke tests
1. Abrir `/core/auth/login`.
2. Login PlatformAdmin → navegar a Tenants.
3. Login TenantAdmin → ver "Mis aplicaciones".
4. Click "Abrir" en Peluquerías/Inventarios.

## Notas
- BaseUrl de módulos vive en `ModuleCatalog` y se puede ajustar desde Platform Admin.
- SSO/OIDC pendiente para fase siguiente.
