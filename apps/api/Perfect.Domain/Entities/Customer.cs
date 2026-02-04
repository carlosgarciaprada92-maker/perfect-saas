using Perfect.Domain.Common;

namespace Perfect.Domain.Entities;

public class Customer : Entity, ITenantEntity, IAuditable
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Identification { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public int DefaultCreditDays { get; set; } = 15;
    public decimal? CreditLimit { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedByUserId { get; set; }
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
