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
- `CORE_DEFAULT_MODULE_BASEURL` (opcional, para seed de URLs)

## Rutas principales

### Core (frontend)
- `/core/auth/login`
- `/core/portal/login`
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
- `http://3.143.255.25/`
- `http://3.143.255.25/core/auth/login` (Platform Console)
- `http://3.143.255.25/core/portal/login` (Customer Portal)
- `http://3.143.255.25/core/workspace`
- `http://3.143.255.25/core/platform/tenants`
- `http://3.143.255.25/api/health`
- `http://3.143.255.25/swagger/`

## Credenciales demo
- PlatformAdmin: `platform.admin@perfect.demo` / `Platform123!` (tenant fijo: `platform`)
- TenantAdmin: `admin@perfect.demo` / `Admin123!` (tenant: `demo`)

## Smoke tests
1. Abrir `/core/auth/login` y verificar texto de Platform Console.
2. Login PlatformAdmin → navegar a Tenants.
3. Abrir `/core/portal/login` y verificar texto de Customer Portal.
4. Login TenantAdmin → ver "Mis aplicaciones".
5. Click "Abrir" en Peluquerías/Inventarios.

## QA checklist (regresión)
1. `GET /core/auth/login` devuelve 200 y muestra textos humanos (sin llaves i18n).
2. Cambiar idioma ES/EN desde login y topbar: debe traducir toda la UI; si falta una traducción, cae a ES.
3. `/core/auth/login` fuerza tenant `platform` (no editable).
4. Login PlatformAdmin → `/core/platform/tenants` y la tabla renderiza al primer load (sin interacción extra).
5. `/core/platform/modules` muestra URL base editable; guardar cambios persiste.
6. `/core/platform/assignments` muestra URL y botón copiar funciona.
7. Login TenantAdmin → `/core/workspace` muestra apps habilitadas.
8. Botón “Abrir” con URL vacía → toast “URL no configurada”.
9. Botón “Abrir” con URL no accesible → toast de error.
10. Botón “Abrir” con URL válida → abre nueva pestaña.

## Notas
- BaseUrl de módulos vive en `ModuleCatalog` y se puede ajustar desde Platform Admin.
- En DEV la IP pública puede cambiar tras redeploy; actualizar `CORE_DEFAULT_MODULE_BASEURL` o editar módulos desde `/core/platform/modules`.
- SSO/OIDC pendiente para fase siguiente.




