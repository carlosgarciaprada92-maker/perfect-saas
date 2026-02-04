# Deployment Guide

## 1) Local development

### Web
```bash
cd apps/web
npm ci
npm run build
```

### API
```bash
cd apps/api
dotnet build Perfect.sln
dotnet test Perfect.sln
```

### Run API + DB
```bash
docker compose up -d --build
```

Check:
- `http://localhost:8080/health`
- `http://localhost:8080/swagger`

## 2) Build API image for AWS

Manual local build:
```bash
docker build -f apps/api/Perfect.Api/Dockerfile -t perfect-api:latest apps/api
```

CI manual publish:
- Go to GitHub Actions
- Run workflow `Docker API Publish`
- Provide `image_tag`
- Ensure secrets exist:
  - `AWS_ACCESS_KEY_ID`
  - `AWS_SECRET_ACCESS_KEY`
  - `ECR_REPOSITORY`

## 3) Terraform plan

### Dev
```bash
cd infra/terraform/envs/dev
cp terraform.tfvars.example terraform.tfvars
# edit db_password and others
terraform init -backend=false
terraform fmt -recursive
terraform validate
terraform plan
```

### Prod
```bash
cd infra/terraform/envs/prod
cp terraform.tfvars.example terraform.tfvars
terraform init -backend=false
terraform validate
terraform plan
```

## 4) Remote state (future)

Use templates in each env:
- `backend.tf` (uncomment backend block)
- `backend.hcl.example` -> `backend.hcl`

Then:
```bash
terraform init -backend-config=backend.hcl
```

## 5) Future steps (domain + HTTPS)

Planned next iteration:
1. Add Route53 hosted zone and ACM certificate.
2. Add ALB + HTTPS listener (or API Gateway) in front of ECS service.
3. Add WAF rules and private networking hardening.
4. Move secrets to AWS Secrets Manager / SSM Parameter Store.
5. Add blue/green deployment strategy.