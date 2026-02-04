# Terraform - Perfect SaaS

This folder contains AWS Terraform code for a low-cost MVP deployment baseline:

- `network`: VPC + subnets + security groups
- `ecr`: API image repository
- `rds`: PostgreSQL (RDS micro)
- `ecs`: Fargate cluster/service for API
- `observability`: basic CloudWatch alarms

## Environments
- `envs/dev`
- `envs/prod`

## Quick start

```bash
cd infra/terraform/envs/dev
terraform init
terraform plan -var="db_password=change-me"
```

For remote state (recommended), use templates:

```bash
terraform init -backend-config=backend.hcl
```

Create `backend.hcl` from `backend.hcl.example` after provisioning S3 and DynamoDB.

## Notes
- These templates are ready for planning/validation.
- They intentionally do **not** auto-create remote-state bucket/lock table.
- Secrets should move to AWS Secrets Manager or SSM Parameter Store before production.
