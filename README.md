# Perfect Monorepo

Monorepo SaaS de **Perfect** para Inventario + Ventas + Cartera.

## Estructura

```text
perfect/
  apps/
    web/                  # Angular frontend
    api/                  # .NET backend
  packages/
    shared/               # contratos/shared (placeholder)
  infra/
    terraform/
      envs/
        dev/
        prod/
      modules/
        network/
        ecs/
        rds/
        ecr/
        observability/
  docs/
    architecture/
    runbooks/
  scripts/
  .github/
    workflows/
  docker/
```

## Requisitos
- Node.js 22+
- npm 10+
- .NET SDK 8+
- Docker + Docker Compose
- Terraform 1.6+

## Ejecutar local

### Frontend
```bash
cd apps/web
npm ci
npm run build
```

### Backend
```bash
cd apps/api
dotnet build Perfect.sln
dotnet test Perfect.sln
```

### API + DB con Docker
```bash
docker compose up -d --build
```

Servicios:
- API: `http://localhost:8080`
- Swagger: `http://localhost:8080/swagger`
- Postgres local: `localhost:5433`

### Seed demo backend
```bash
curl -X POST http://localhost:8080/api/v1/auth/seed-demo
```

Credenciales demo:
- `admin@perfect.demo` / `Admin123!`
- `ventas@perfect.demo` / `Ventas123!`
- `bodega@perfect.demo` / `Bodega123!`

## Variables clave

### apps/web
- `PERFECT_API_BASE_URL` (runtime global opcional) o `src/environments/environment.ts`

### apps/api
- `ConnectionStrings__Default`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Jwt__Key`
- `Tenant__Mode`
- `Tenant__HeaderName`
- `Cors__AllowedOrigins`

## Terraform
```bash
cd infra/terraform
terraform fmt -recursive

cd envs/dev
terraform init -backend=false
terraform validate
```

Ver guía completa en `docs/DEPLOYMENT.md`.

## CI/CD
- `.github/workflows/ci.yml`: build/test web+api
- `.github/workflows/docker.yml`: build/push imagen API a ECR (manual, `workflow_dispatch`)

## Estado remoto Terraform (plantilla)
Cada entorno incluye:
- `backend.tf` (comentado)
- `backend.hcl.example` (S3 + DynamoDB lock)

## Nota AWS
Sesión AWS validada localmente con `aws sts get-caller-identity`.
No se ejecuta `terraform apply` automáticamente.