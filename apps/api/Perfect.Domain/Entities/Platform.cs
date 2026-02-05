using Perfect.Domain.Common;
using Perfect.Domain.Enums;

namespace Perfect.Domain.Entities;

public class ModuleCatalog : Entity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public ModuleStatus Status { get; set; } = ModuleStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<TenantModule> TenantModules { get; set; } = new List<TenantModule>();
}

public class TenantModule : Entity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid ModuleId { get; set; }
    public ModuleCatalog Module { get; set; } = null!;
    public bool Enabled { get; set; }
    public DateTime? ActivatedAt { get; set; }
    public string? Notes { get; set; }
}
