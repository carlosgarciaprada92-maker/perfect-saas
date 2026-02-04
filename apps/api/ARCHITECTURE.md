# Architecture - Perfect Backend

## 1. Multi-tenancy strategy
Current strategy: **single database / shared schema / TenantId column**.

### Tenant resolution
`TenantResolutionMiddleware` resolves tenant in this order:
1. `X-Tenant-Id`
2. `X-Tenant-Slug`
3. subdomain (`tenant.perfect.com`)
4. JWT claim `tenantId`

Resolved tenant is stored in a scoped `TenantContext` implementing `ITenantProvider`.

### Isolation enforcement
- Every business/security entity includes `TenantId` (except global catalogs like `Permission`, and `Tenant`).
- `AppDbContext` applies **Global Query Filters** for all `ITenantEntity` types.
- `TenantSaveChangesInterceptor` auto-populates `TenantId` on inserts and audit fields.
- Tests validate that cross-tenant invoice access returns not found and query filters isolate data.

## 2. Security model (RBAC)
Entities:
- `User`, `Role`, `Permission`, `UserRole`, `RolePermission`, `RefreshToken`

Permissions are module/action based (e.g. `products.read`, `invoices.write`, `reports.read`).

### Auth flow
- Login issues JWT access token and refresh token.
- Access token includes claims: `sub`, `tenantId`, `email`, `role`, `perm`.
- Refresh token rotation implemented (`ReplacedByToken`, revoked old token).
- Logout revokes refresh token.

### Authorization
- `RequirePermissionAttribute` validates `perm` claim (or ADMIN role).
- `RequireTenantAttribute` + middleware ensures tenant context for protected endpoints.

## 3. Domain and AR rules
Key entities:
- Products, inventory movements, customers, invoices, invoice items, payments.

### Invoice rules
- `balance = total - paidTotal`
- `Cash`: due date = invoice date, typically paid immediately.
- `Credit`: due date = `invoice.date + creditDaysApplied`
- Status (computed):
  - `Paid` if balance <= 0
  - `Overdue` if now > dueDate and balance > 0
  - `DueSoon` if balance > 0 and daysToDue <= threshold
  - `Pending` otherwise

This fixes the previous bug where credit sales could be treated as settled incorrectly.

## 4. Application/service layer
Services expose use-case style operations:
- Auth, Tenants, Products, Inventory, Customers, Invoices, AR, Reports, Users/Roles.

Validation:
- FluentValidation for auth/invoice/payment and key business requests.

Reporting:
- AR summary, due-soon, overdue, open items.
- Sales summary, sales by day, sales by payment type.
- Top customers, inactive customers, overdue customers aggregation.
- CSV export from `/api/v1/reports/export`.

## 5. Observability and operational concerns
- Structured logging via Serilog.
- Correlation id middleware (`X-Correlation-Id`).
- Health check endpoint `/health`.
- API versioning by route prefix (`/api/v1/...`).
- Swagger with bearer auth configured.

## 6. Deployment model
- Dockerfile for API.
- `docker-compose.yml` for API + PostgreSQL.
- API runs migrations automatically on startup.

## 7. Tradeoffs and next steps
### Current tradeoffs
- Tenant creation currently bootstrap-protected by `Platform__BootstrapKey` header; no full platform-admin user domain yet.
- Permission catalog is global and seeded centrally.
- No caching layer yet for heavy report queries.

### Recommended next steps
1. Add true platform-admin module and tenant lifecycle APIs (plan changes, suspension, billing hooks).
2. Add optional `database-per-tenant` resolver (connection-per-tenant strategy).
3. Add idempotency keys for invoice creation and payment registration.
4. Add distributed cache + query materialization for dashboard/report hot paths.
5. Add audit log table (append-only) for compliance-grade traceability.