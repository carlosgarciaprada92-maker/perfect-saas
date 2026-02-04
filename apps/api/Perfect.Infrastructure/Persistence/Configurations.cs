using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Perfect.Domain.Entities;

namespace Perfect.Infrastructure.Persistence;

public class TenantConfig : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.HasIndex(x => x.Slug).IsUnique();
        builder.Property(x => x.Name).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(120).IsRequired();
    }
}

public class TenantSettingsConfig : IEntityTypeConfiguration<TenantSettings>
{
    public void Configure(EntityTypeBuilder<TenantSettings> builder)
    {
        builder.HasOne<Tenant>().WithOne(t => t.Settings).HasForeignKey<TenantSettings>(x => x.TenantId);
        builder.Property(x => x.Currency).HasMaxLength(12).IsRequired();
        builder.Property(x => x.Timezone).HasMaxLength(100).IsRequired();
        builder.Property(x => x.InvoiceNumberingFormat).HasMaxLength(32).IsRequired();
        builder.Property(x => x.AdministrativeFeePercent).HasPrecision(8, 2);
    }
}

public class UserConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(x => new { x.TenantId, x.Email }).IsUnique();
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(160).IsRequired();
    }
}

public class RoleConfig : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
    }
}

public class PermissionConfig : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.HasIndex(x => x.Code).IsUnique();
    }
}

public class ProductConfig : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasIndex(x => new { x.TenantId, x.Sku }).IsUnique();
        builder.Property(x => x.Price).HasPrecision(18, 2);
        builder.Property(x => x.Cost).HasPrecision(18, 2);
    }
}

public class CustomerConfig : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasIndex(x => new { x.TenantId, x.Identification }).IsUnique();
        builder.Property(x => x.CreditLimit).HasPrecision(18, 2);
    }
}

public class InvoiceConfig : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.HasIndex(x => new { x.TenantId, x.Number }).IsUnique();
        builder.Property(x => x.Total).HasPrecision(18, 2);
        builder.Property(x => x.PaidTotal).HasPrecision(18, 2);
        builder.Property(x => x.Balance).HasPrecision(18, 2);
        builder.HasMany(x => x.Items).WithOne(x => x.Invoice).HasForeignKey(x => x.InvoiceId);
        builder.HasMany(x => x.Payments).WithOne(x => x.Invoice).HasForeignKey(x => x.InvoiceId);
    }
}

public class InvoiceItemConfig : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        builder.Property(x => x.UnitPrice).HasPrecision(18, 2);
        builder.Property(x => x.Total).HasPrecision(18, 2);
    }
}

public class PaymentConfig : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.Property(x => x.Amount).HasPrecision(18, 2);
    }
}
