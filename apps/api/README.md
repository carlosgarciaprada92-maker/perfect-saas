# Perfect API

Backend .NET 8 para Perfect SaaS (multi-tenant).

## Comandos
```bash
dotnet build Perfect.sln
dotnet test Perfect.sln
```

## Variables principales
- `ConnectionStrings__Default`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Jwt__Key`
- `Tenant__Mode`
- `Tenant__HeaderName`
- `Tenant__SlugHeaderName`
- `Cors__AllowedOrigins`

## Docker local
Desde la raíz del monorepo:
```bash
docker compose up -d --build
```