using Perfect.Domain.Common;
using Perfect.Domain.Enums;

namespace Perfect.Domain.Entities;

public class Invoice : Entity, ITenantEntity, IAuditable
{
    public Guid TenantId { get; set; }
    public string Number { get; set; } = string.Empty;
    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public PaymentType PaymentType { get; set; } = PaymentType.Cash;
    public int CreditDaysApplied { get; set; }
    public DateTime DueDate { get; set; } = DateTime.UtcNow;
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;
    public decimal Total { get; set; }
    public decimal PaidTotal { get; set; }
    public decimal Balance { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

public class InvoiceItem : Entity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }
}

public class Payment : Entity, ITenantEntity, IAuditable
{
    public Guid TenantId { get; set; }
    public Guid InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public decimal Amount { get; set; }
    public string Method { get; set; } = "CASH";
    public string? Reference { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
