using Microsoft.EntityFrameworkCore;
using Perfect.Application.Common;
using Perfect.Application.Services;
using Perfect.Domain.Entities;
using Perfect.Domain.Enums;
using Perfect.Infrastructure.MultiTenancy;
using Perfect.Infrastructure.Persistence;

namespace Perfect.Infrastructure.Services;

public class DemoSeedService : IDemoSeedService
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly TenantContext _tenantContext;
    private readonly IInvoiceStatusCalculator _calculator;
    private readonly IDateTimeProvider _clock;

    public DemoSeedService(AppDbContext db, IPasswordHasher passwordHasher, TenantContext tenantContext, IInvoiceStatusCalculator calculator, IDateTimeProvider clock)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _tenantContext = tenantContext;
        _calculator = calculator;
        _clock = clock;
    }

    public async Task SeedDemoAsync(string? slug = null, CancellationToken ct = default)
    {
        await DbInitializer.EnsureCatalogAsync(_db, ct);

        var tenantSlug = string.IsNullOrWhiteSpace(slug) ? "demo" : slug.Trim().ToLowerInvariant();
        var tenant = await DbInitializer.EnsureTenantAsync(_db, "Perfect Demo", tenantSlug, ct);
        var platformTenant = await DbInitializer.EnsureTenantAsync(_db, "Perfect Platform", "platform", ct);
        await SeedPlatformAsync(platformTenant, ct);

        _tenantContext.SetTenant(tenant.Id, tenant.Slug);

        await SeedRolesAsync(tenant.Id, ct);
        await SeedUsersAsync(tenant.Id, ct);
        await SeedTenantModulesAsync(tenant.Id, ct);

        if (!await _db.Products.AnyAsync(ct))
        {
            var products = Enumerable.Range(1, 36).Select(i => new Product
            {
                Sku = $"SKU-{i:000}",
                Name = $"Producto {i:00}",
                Description = $"Producto demo {i}",
                Price = 10000 + i * 1500,
                Cost = 7000 + i * 800,
                MinStock = 5 + (i % 8),
                IsActive = true
            }).ToList();
            _db.Products.AddRange(products);
            await _db.SaveChangesAsync(ct);
        }

        if (!await _db.Customers.AnyAsync(ct))
        {
            var terms = new[] { 7, 15, 30, 45 };
            var customers = Enumerable.Range(1, 24).Select(i => new Customer
            {
                Name = $"Cliente Demo {i:00}",
                Identification = $"90{i:000000}",
                Phone = $"30055{i:0000}",
                Email = $"cliente{i:00}@perfect.demo",
                DefaultCreditDays = terms[i % terms.Length],
                CreditLimit = 2_000_000 + i * 150_000,
                IsActive = true,
                CreatedAt = _clock.UtcNow.AddDays(-120 + i)
            }).ToList();

            _db.Customers.AddRange(customers);
            await _db.SaveChangesAsync(ct);
        }

        if (!await _db.Invoices.AnyAsync(ct))
        {
            var rnd = new Random(20260204);
            var products = await _db.Products.Take(36).ToListAsync(ct);
            var customers = await _db.Customers.Take(24).ToListAsync(ct);

            var invoices = new List<Invoice>();
            for (var i = 1; i <= 52; i++)
            {
                var type = i % 4 == 0 ? PaymentType.Cash : PaymentType.Credit;
                var customer = customers[rnd.Next(customers.Count)];
                var date = _clock.UtcNow.Date.AddDays(-rnd.Next(0, 70));
                var creditDays = type == PaymentType.Credit ? (i % 5 == 0 ? 7 : i % 3 == 0 ? 15 : i % 2 == 0 ? 30 : 45) : 0;
                var dueDate = _calculator.ResolveDueDate(date, type, creditDays);

                if (type == PaymentType.Credit)
                {
                    if (i % 6 == 0)
                    {
                        dueDate = _clock.UtcNow.Date.AddDays(-rnd.Next(5, 20));
                    }
                    else if (i % 4 == 0)
                    {
                        dueDate = _clock.UtcNow.Date.AddDays(rnd.Next(1, 5));
                    }
                    else
                    {
                        dueDate = _clock.UtcNow.Date.AddDays(rnd.Next(10, 30));
                    }
                }

                var invoice = new Invoice
                {
                    Number = $"FAC-{i:0000}",
                    CustomerId = customer.Id,
                    Date = date,
                    PaymentType = type,
                    CreditDaysApplied = creditDays,
                    DueDate = dueDate,
                    CreatedByUserId = Guid.Empty
                };

                var linesCount = rnd.Next(1, 4);
                for (var l = 0; l < linesCount; l++)
                {
                    var product = products[rnd.Next(products.Count)];
                    var qty = rnd.Next(1, 5);
                    var price = product.Price;
                    invoice.Items.Add(new InvoiceItem
                    {
                        ProductId = product.Id,
                        Quantity = qty,
                        UnitPrice = price,
                        Total = price * qty
                    });
                    _db.InventoryMovements.Add(new InventoryMovement
                    {
                        ProductId = product.Id,
                        Type = InventoryMovementType.Out,
                        Quantity = qty,
                        Reason = $"Demo Invoice {invoice.Number}",
                        CreatedByUserId = Guid.Empty
                    });
                }

                invoice.Total = invoice.Items.Sum(x => x.Total);
                invoice.PaidTotal = 0;
                invoice.Balance = invoice.Total;

                if (type == PaymentType.Cash)
                {
                    invoice.PaidTotal = invoice.Total;
                    invoice.Balance = 0;
                    invoice.Status = InvoiceStatus.Paid;
                    invoice.Payments.Add(new Payment
                    {
                        Date = invoice.Date,
                        Amount = invoice.Total,
                        Method = "CASH",
                        CreatedByUserId = Guid.Empty
                    });
                }
                else
                {
                    if (i % 7 == 0)
                    {
                        var partial = decimal.Round(invoice.Total * 0.4m, 2);
                        invoice.PaidTotal = partial;
                        invoice.Balance = invoice.Total - partial;
                        invoice.Payments.Add(new Payment
                        {
                            Date = invoice.Date.AddDays(2),
                            Amount = partial,
                            Method = "TRANSFER",
                            CreatedByUserId = Guid.Empty
                        });
                    }

                    if (i % 11 == 0)
                    {
                        invoice.PaidTotal = invoice.Total;
                        invoice.Balance = 0;
                    }

                    invoice.Status = _calculator.ResolveStatus(invoice.Balance, invoice.DueDate, _clock.UtcNow, 5);
                }

                invoices.Add(invoice);
            }

            _db.Invoices.AddRange(invoices);
            await _db.SaveChangesAsync(ct);
        }
    }

    private async Task SeedPlatformAsync(Tenant platformTenant, CancellationToken ct)
    {
        _tenantContext.SetTenant(platformTenant.Id, platformTenant.Slug);

        var platformRole = await EnsureRoleAsync(platformTenant.Id, "PlatformAdmin", ct, true);
        var platformUser = await EnsureUserAsync(platformTenant.Id, "Platform Admin", "platform.admin@perfect.demo", "Platform123!", ct);
        await EnsureUserRoleAsync(platformTenant.Id, platformUser.Id, platformRole.Id, ct);
    }

    private async Task SeedRolesAsync(Guid tenantId, CancellationToken ct)
    {
        var owner = await EnsureRoleAsync(tenantId, "ADMIN", ct, true);
        var tenantAdmin = await EnsureRoleAsync(tenantId, "TenantAdmin", ct, true);
        var ventas = await EnsureRoleAsync(tenantId, "Ventas", ct, true);
        var bodega = await EnsureRoleAsync(tenantId, "Bodega", ct, true);

        var perms = await _db.Permissions.ToListAsync(ct);
        await EnsureRolePermissionsAsync(tenantId, owner.Id, perms.Select(p => p.Id), ct);
        await EnsureRolePermissionsAsync(tenantId, tenantAdmin.Id, perms.Select(p => p.Id), ct);

        foreach (var code in new[] { "products.read", "products.write", "customers.read", "customers.write", "invoices.read", "invoices.write", "payments.write", "ar.read", "reports.read" })
        {
            var permission = perms.First(p => p.Code == code);
            await EnsureRolePermissionsAsync(tenantId, ventas.Id, new[] { permission.Id }, ct);
        }

        foreach (var code in new[] { "products.read", "products.write", "inventory.write" })
        {
            var permission = perms.First(p => p.Code == code);
            await EnsureRolePermissionsAsync(tenantId, bodega.Id, new[] { permission.Id }, ct);
        }
    }

    private async Task SeedUsersAsync(Guid tenantId, CancellationToken ct)
    {
        var ownerRole = await _db.Roles.FirstAsync(r => r.Name == "ADMIN", ct);
        var tenantAdminRole = await _db.Roles.FirstAsync(r => r.Name == "TenantAdmin", ct);
        var ventasRole = await _db.Roles.FirstAsync(r => r.Name == "Ventas", ct);
        var bodegaRole = await _db.Roles.FirstAsync(r => r.Name == "Bodega", ct);

        var admin = await EnsureUserAsync(tenantId, "Admin Demo", "admin@perfect.demo", "Admin123!", ct);
        var ventas = await EnsureUserAsync(tenantId, "Ventas Demo", "ventas@perfect.demo", "Ventas123!", ct);
        var bodega = await EnsureUserAsync(tenantId, "Bodega Demo", "bodega@perfect.demo", "Bodega123!", ct);

        await EnsureUserRoleAsync(tenantId, admin.Id, ownerRole.Id, ct);
        await EnsureUserRoleAsync(tenantId, admin.Id, tenantAdminRole.Id, ct);
        await EnsureUserRoleAsync(tenantId, ventas.Id, ventasRole.Id, ct);
        await EnsureUserRoleAsync(tenantId, bodega.Id, bodegaRole.Id, ct);
    }

    private async Task SeedTenantModulesAsync(Guid tenantId, CancellationToken ct)
    {
        var modules = await _db.ModuleCatalogs.AsNoTracking()
            .Where(x => x.Slug == "peluquerias" || x.Slug == "inventarios")
            .ToListAsync(ct);
        var existing = await _db.TenantModules.IgnoreQueryFilters().Where(x => x.TenantId == tenantId).ToListAsync(ct);

        foreach (var module in modules)
        {
            var current = existing.FirstOrDefault(x => x.ModuleId == module.Id);
            if (current != null)
            {
                if (!current.Enabled)
                {
                    current.Enabled = true;
                    current.ActivatedAt ??= _clock.UtcNow;
                }
                continue;
            }

            _db.TenantModules.Add(new TenantModule
            {
                TenantId = tenantId,
                ModuleId = module.Id,
                Enabled = true,
                ActivatedAt = _clock.UtcNow
            });
        }

        await _db.SaveChangesAsync(ct);
    }

    private async Task<Role> EnsureRoleAsync(Guid tenantId, string name, CancellationToken ct, bool isSystem)
    {
        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == name, ct);
        if (role != null)
        {
            return role;
        }

        role = new Role { TenantId = tenantId, Name = name, IsSystemRole = isSystem };
        _db.Roles.Add(role);
        await _db.SaveChangesAsync(ct);
        return role;
    }

    private async Task<User> EnsureUserAsync(Guid tenantId, string name, string email, string password, CancellationToken ct)
    {
        var normalized = email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == normalized, ct);
        if (user != null)
        {
            return user;
        }

        user = new User
        {
            TenantId = tenantId,
            Name = name,
            Email = normalized,
            PasswordHash = _passwordHasher.Hash(password),
            IsActive = true
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);
        return user;
    }

    private async Task EnsureUserRoleAsync(Guid tenantId, Guid userId, Guid roleId, CancellationToken ct)
    {
        var exists = await _db.UserRoles.AnyAsync(x => x.UserId == userId && x.RoleId == roleId, ct);
        if (exists)
        {
            return;
        }

        _db.UserRoles.Add(new UserRole { TenantId = tenantId, UserId = userId, RoleId = roleId });
        await _db.SaveChangesAsync(ct);
    }

    private async Task EnsureRolePermissionsAsync(Guid tenantId, Guid roleId, IEnumerable<Guid> permissionIds, CancellationToken ct)
    {
        var existing = await _db.RolePermissions.Where(x => x.RoleId == roleId).Select(x => x.PermissionId).ToListAsync(ct);
        var missing = permissionIds.Where(id => !existing.Contains(id)).ToList();
        if (missing.Count == 0)
        {
            return;
        }

        foreach (var permissionId in missing)
        {
            _db.RolePermissions.Add(new RolePermission { TenantId = tenantId, RoleId = roleId, PermissionId = permissionId });
        }

        await _db.SaveChangesAsync(ct);
    }
}
