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
        _tenantContext.SetTenant(tenant.Id, tenant.Slug);

        await SeedRolesAsync(tenant.Id, ct);
        await SeedUsersAsync(tenant.Id, ct);

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

    private async Task SeedRolesAsync(Guid tenantId, CancellationToken ct)
    {
        if (await _db.Roles.AnyAsync(ct))
        {
            return;
        }

        var owner = new Role { TenantId = tenantId, Name = "ADMIN", IsSystemRole = true };
        var ventas = new Role { TenantId = tenantId, Name = "Ventas", IsSystemRole = true };
        var bodega = new Role { TenantId = tenantId, Name = "Bodega", IsSystemRole = true };

        _db.Roles.AddRange(owner, ventas, bodega);
        await _db.SaveChangesAsync(ct);

        var perms = await _db.Permissions.ToListAsync(ct);

        foreach (var permission in perms)
        {
            _db.RolePermissions.Add(new RolePermission { TenantId = tenantId, RoleId = owner.Id, PermissionId = permission.Id });
        }

        foreach (var code in new[] { "products.read", "products.write", "customers.read", "customers.write", "invoices.read", "invoices.write", "payments.write", "ar.read", "reports.read" })
        {
            var permission = perms.First(p => p.Code == code);
            _db.RolePermissions.Add(new RolePermission { TenantId = tenantId, RoleId = ventas.Id, PermissionId = permission.Id });
        }

        foreach (var code in new[] { "products.read", "products.write", "inventory.write" })
        {
            var permission = perms.First(p => p.Code == code);
            _db.RolePermissions.Add(new RolePermission { TenantId = tenantId, RoleId = bodega.Id, PermissionId = permission.Id });
        }

        await _db.SaveChangesAsync(ct);
    }

    private async Task SeedUsersAsync(Guid tenantId, CancellationToken ct)
    {
        if (await _db.Users.AnyAsync(ct))
        {
            return;
        }

        var ownerRole = await _db.Roles.FirstAsync(r => r.Name == "ADMIN", ct);
        var ventasRole = await _db.Roles.FirstAsync(r => r.Name == "Ventas", ct);
        var bodegaRole = await _db.Roles.FirstAsync(r => r.Name == "Bodega", ct);

        var users = new[]
        {
            new User { TenantId = tenantId, Name = "Admin Demo", Email = "admin@perfect.demo", PasswordHash = _passwordHasher.Hash("Admin123!"), IsActive = true },
            new User { TenantId = tenantId, Name = "Ventas Demo", Email = "ventas@perfect.demo", PasswordHash = _passwordHasher.Hash("Ventas123!"), IsActive = true },
            new User { TenantId = tenantId, Name = "Bodega Demo", Email = "bodega@perfect.demo", PasswordHash = _passwordHasher.Hash("Bodega123!"), IsActive = true }
        };

        _db.Users.AddRange(users);
        await _db.SaveChangesAsync(ct);

        _db.UserRoles.AddRange(
            new UserRole { TenantId = tenantId, UserId = users[0].Id, RoleId = ownerRole.Id },
            new UserRole { TenantId = tenantId, UserId = users[1].Id, RoleId = ventasRole.Id },
            new UserRole { TenantId = tenantId, UserId = users[2].Id, RoleId = bodegaRole.Id }
        );

        await _db.SaveChangesAsync(ct);
    }
}
