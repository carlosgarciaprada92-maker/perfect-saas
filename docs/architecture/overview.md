# System Overview

Perfect is organized as a SaaS monorepo with three layers:

1. **Applications**
- `apps/web`: Angular UI
- `apps/api`: .NET 8 API with multi-tenant RBAC

2. **Shared packages**
- `packages/shared`: future contracts/utilities

3. **Infrastructure as Code**
- `infra/terraform`: AWS modules/environments for MVP deployment

## Multi-tenant model (API)
- Tenant resolution by header/slug/subdomain/claim.
- `TenantId` query filter in EF Core.
- SaveChanges interceptor sets audit and tenant ownership fields.

## AWS target (MVP low-cost)
- ECS Fargate for API
- RDS PostgreSQL micro (single AZ)
- ECR for images
- CloudWatch logs/alarms

## CI baseline
- Web build + API build/test in CI.
- Manual Docker publish workflow for ECR.