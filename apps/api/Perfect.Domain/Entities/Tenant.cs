using Perfect.Domain.Common;
using Perfect.Domain.Enums;

namespace Perfect.Domain.Entities;

public class Tenant : Entity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public TenantStatus Status { get; set; } = TenantStatus.Active;
    public string Plan { get; set; } = "Standard";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public TenantSettings? Settings { get; set; }
}

public class TenantSettings : Entity, Perfect.Domain.Common.ITenantEntity
{
    public Guid TenantId { get; set; }
    public string Currency { get; set; } = "COP";
    public string Timezone { get; set; } = "America/Bogota";
    public int DefaultCreditDays { get; set; } = 15;
    public int DueSoonThresholdDays { get; set; } = 5;
    public int LowStockThreshold { get; set; } = 5;
    public decimal AdministrativeFeePercent { get; set; } = 2m;
    public string InvoiceNumberingFormat { get; set; } = "FAC-{0000}";
}
