namespace Perfect.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; set; } = Guid.NewGuid();
}

public interface ITenantEntity
{
    Guid TenantId { get; set; }
}

public interface IAuditable
{
    DateTime CreatedAt { get; set; }
    Guid? CreatedByUserId { get; set; }
    DateTime? UpdatedAt { get; set; }
    Guid? UpdatedByUserId { get; set; }
}
