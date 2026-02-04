# Local Runbook

## Start full local stack
```bash
# from monorepo root
docker compose up -d --build
```

## Validate backend
```bash
curl http://localhost:8080/health
curl -X POST http://localhost:8080/api/v1/auth/seed-demo
```

## Validate web build
```bash
cd apps/web
npm ci
npm run build
```

## Stop stack
```bash
docker compose down
```