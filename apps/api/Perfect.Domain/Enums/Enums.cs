namespace Perfect.Domain.Enums;

public enum InventoryMovementType
{
    In = 1,
    Out = 2,
    Adjust = 3
}

public enum PaymentType
{
    Cash = 1,
    Credit = 2
}

public enum InvoiceStatus
{
    Pending = 1,
    DueSoon = 2,
    Overdue = 3,
    Paid = 4
}

public enum TenantStatus
{
    Active = 1,
    Suspended = 2
}
