namespace Perfect.Application.Contracts;

public record TenantCreateRequest(string Name, string Slug, string Plan);
public record TenantResponse(Guid Id, string Name, string Slug, string Status, string Plan, DateTime CreatedAt);
public record TenantSettingsRequest(
    string Currency,
    string Timezone,
    int DefaultCreditDays,
    int DueSoonThresholdDays,
    int LowStockThreshold,
    decimal AdministrativeFeePercent,
    string InvoiceNumberingFormat);

public record TenantSettingsResponse(
    Guid TenantId,
    string Currency,
    string Timezone,
    int DefaultCreditDays,
    int DueSoonThresholdDays,
    int LowStockThreshold,
    decimal AdministrativeFeePercent,
    string InvoiceNumberingFormat);
