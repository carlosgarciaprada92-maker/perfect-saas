namespace Perfect.Application.Contracts;

public record ProductRequest(string Sku, string Name, string? Description, decimal Price, decimal? Cost, int MinStock, bool IsActive);
public record ProductResponse(Guid Id, string Sku, string Name, string? Description, decimal Price, decimal? Cost, int MinStock, bool IsActive);
public record ProductStatusRequest(bool IsActive);

public record CustomerRequest(string Name, string Identification, string Phone, string? Email, int DefaultCreditDays, decimal? CreditLimit, bool IsActive);
public record CustomerResponse(Guid Id, string Name, string Identification, string Phone, string? Email, int DefaultCreditDays, decimal? CreditLimit, bool IsActive);
public record CustomerCreditTermsRequest(int DefaultCreditDays, decimal? CreditLimit);

public record InventoryMovementRequest(Guid ProductId, int Quantity, string? Reason);
public record InventoryMovementResponse(Guid Id, Guid ProductId, string ProductName, string Type, int Quantity, string? Reason, DateTime CreatedAt);

public record InvoiceItemRequest(Guid ProductId, int Quantity, decimal? UnitPrice);
public record InvoiceCreateRequest(Guid? CustomerId, string PaymentType, int? CreditDaysApplied, IReadOnlyCollection<InvoiceItemRequest> Items);
public record InvoiceResponse(Guid Id, string Number, DateTime Date, string PaymentType, int CreditDaysApplied, DateTime DueDate, string Status, decimal Total, decimal PaidTotal, decimal Balance, Guid? CustomerId, string? CustomerName);
public record InvoiceDetailResponse(InvoiceResponse Header, IReadOnlyCollection<InvoiceLineResponse> Items, IReadOnlyCollection<PaymentResponse> Payments);
public record InvoiceLineResponse(Guid Id, Guid ProductId, string ProductName, int Quantity, decimal UnitPrice, decimal Total);
public record PaymentRequest(decimal Amount, string Method, string? Reference);
public record PaymentResponse(Guid Id, DateTime Date, decimal Amount, string Method, string? Reference);

public record UserRequest(string Name, string Email, string Password, bool IsActive);
public record UserUpdateRequest(string Name, string Email, bool IsActive);
public record UserResponse(Guid Id, string Name, string Email, bool IsActive);
public record UserStatusRequest(bool IsActive);
public record RoleRequest(string Name);
public record RoleResponse(Guid Id, string Name, bool IsSystemRole);
public record AssignRoleRequest(IReadOnlyCollection<Guid> RoleIds);
public record PermissionResponse(Guid Id, string Code, string Module, string Action, string Description);
public record AssignPermissionsRequest(IReadOnlyCollection<Guid> PermissionIds);

public record ArSummaryResponse(decimal TotalPending, int OverdueCount, int DueSoonCount);
public record ArItemResponse(Guid InvoiceId, string InvoiceNumber, Guid? CustomerId, string? CustomerName, DateTime DueDate, int DaysToDue, int DaysOverdue, decimal Total, decimal Balance, string Status);
public record ReportSalesSummaryResponse(decimal TotalSales, decimal TotalCash, decimal TotalCredit);
public record ReportTopCustomerResponse(Guid CustomerId, string CustomerName, decimal Total, int Count, decimal AvgTicket, decimal CreditPct, decimal CashPct);
public record ReportCustomerActivityResponse(Guid CustomerId, string CustomerName, DateTime? LastPurchase, int DaysInactive, decimal Total);
public record ReportCreditKpiResponse(decimal TotalPending, int OverdueCount, int DueSoonCount, decimal OverdueAmount, decimal DueSoonAmount);
public record ReportSalesByDayResponse(DateTime Date, int Count, decimal Total);
public record ReportSalesByPaymentTypeResponse(string PaymentType, int Count, decimal Total);
public record ReportOverdueCustomerResponse(Guid CustomerId, string CustomerName, int OverdueInvoices, decimal OverdueBalance, int MaxDaysOverdue, DateTime LastDueDate);
