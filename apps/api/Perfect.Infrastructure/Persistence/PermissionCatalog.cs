using Perfect.Domain.Entities;

namespace Perfect.Infrastructure.Persistence;

public static class PermissionCatalog
{
    public static readonly IReadOnlyCollection<PermissionSeed> All = new List<PermissionSeed>
    {
        new("products.read", "products", "read", "Read products"),
        new("products.write", "products", "write", "Create and update products"),
        new("inventory.write", "inventory", "write", "Register inventory movements"),
        new("customers.read", "customers", "read", "Read customers"),
        new("customers.write", "customers", "write", "Manage customers"),
        new("invoices.read", "invoices", "read", "Read invoices"),
        new("invoices.write", "invoices", "write", "Create invoices"),
        new("payments.write", "payments", "write", "Register payments"),
        new("ar.read", "ar", "read", "Read accounts receivable"),
        new("reports.read", "reports", "read", "Read reports"),
        new("users.manage", "users", "manage", "Manage users"),
        new("roles.manage", "roles", "manage", "Manage roles and permissions"),
        new("tenant.settings", "tenant", "settings", "Manage tenant settings")
    };

    public static IReadOnlyCollection<Permission> ToEntities() =>
        All.Select(x => new Permission
        {
            Code = x.Code,
            Module = x.Module,
            Action = x.Action,
            Description = x.Description
        }).ToList();
}

public record PermissionSeed(string Code, string Module, string Action, string Description);
