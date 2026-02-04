using Perfect.Domain.Common;
using Perfect.Domain.Enums;

namespace Perfect.Domain.Entities;

public class InventoryMovement : Entity, ITenantEntity, IAuditable
{
    public Guid TenantId { get; set; }
    public InventoryMovementType Type { get; set; }
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedByUserId { get; set; }
}
