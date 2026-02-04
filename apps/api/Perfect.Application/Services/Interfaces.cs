using Perfect.Application.Contracts;
using Perfect.Application.Models;

namespace Perfect.Application.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct);
    Task<RefreshResponse> RefreshAsync(RefreshRequest request, CancellationToken ct);
    Task LogoutAsync(string refreshToken, CancellationToken ct);
}

public interface ITenantService
{
    Task<TenantResponse> CreateTenantAsync(TenantCreateRequest request, CancellationToken ct);
    Task<TenantResponse> GetCurrentTenantAsync(CancellationToken ct);
    Task<TenantSettingsResponse> UpdateSettingsAsync(TenantSettingsRequest request, CancellationToken ct);
}

public interface IProductService
{
    Task<PaginatedResult<ProductResponse>> GetAsync(int page, int pageSize, string? search, string? sort, string? order, CancellationToken ct);
    Task<ProductResponse> CreateAsync(ProductRequest request, CancellationToken ct);
    Task<ProductResponse> UpdateAsync(Guid id, ProductRequest request, CancellationToken ct);
    Task<ProductResponse> UpdateStatusAsync(Guid id, bool isActive, CancellationToken ct);
    Task<IReadOnlyCollection<ProductResponse>> GetLowStockAsync(int? threshold, CancellationToken ct);
}

public interface ICustomerService
{
    Task<PaginatedResult<CustomerResponse>> GetAsync(int page, int pageSize, string? search, CancellationToken ct);
    Task<CustomerResponse> CreateAsync(CustomerRequest request, CancellationToken ct);
    Task<CustomerResponse> UpdateAsync(Guid id, CustomerRequest request, CancellationToken ct);
    Task<CustomerResponse> UpdateCreditTermsAsync(Guid id, CustomerCreditTermsRequest request, CancellationToken ct);
    Task<IReadOnlyCollection<CustomerResponse>> GetInactiveAsync(int days, CancellationToken ct);
}

public interface IInventoryService
{
    Task<InventoryMovementResponse> InventoryInAsync(InventoryMovementRequest request, CancellationToken ct);
    Task<InventoryMovementResponse> InventoryAdjustAsync(InventoryMovementRequest request, CancellationToken ct);
    Task<PaginatedResult<InventoryMovementResponse>> GetMovementsAsync(int page, int pageSize, CancellationToken ct);
}

public interface IInvoiceService
{
    Task<InvoiceResponse> CreateInvoiceAsync(InvoiceCreateRequest request, CancellationToken ct);
    Task<PaginatedResult<InvoiceResponse>> GetInvoicesAsync(int page, int pageSize, string? search, string? status, string? paymentType, Guid? customerId, DateTime? from, DateTime? to, CancellationToken ct);
    Task<InvoiceDetailResponse> GetInvoiceAsync(Guid id, CancellationToken ct);
    Task<PaymentResponse> RegisterPaymentAsync(Guid id, PaymentRequest request, CancellationToken ct);
    Task<InvoiceResponse> MarkPaidAsync(Guid id, CancellationToken ct);
}

public interface IArService
{
    Task<ArSummaryResponse> GetSummaryAsync(int rangeDays, int thresholdDays, bool includePaid, Guid? customerId, CancellationToken ct);
    Task<IReadOnlyCollection<ArItemResponse>> GetDueSoonAsync(int thresholdDays, CancellationToken ct);
    Task<IReadOnlyCollection<ArItemResponse>> GetOverdueAsync(CancellationToken ct);
    Task<PaginatedResult<ArItemResponse>> GetOpenItemsAsync(
        int page,
        int pageSize,
        string? status,
        bool includePaid,
        Guid? customerId,
        int thresholdDays,
        string? search,
        int? rangeDays,
        CancellationToken ct);
}

public interface IReportService
{
    Task<ReportSalesSummaryResponse> GetSalesSummaryAsync(int rangeDays, CancellationToken ct);
    Task<IReadOnlyCollection<ReportTopCustomerResponse>> GetTopCustomersAsync(int rangeDays, int topN, CancellationToken ct);
    Task<IReadOnlyCollection<ReportCustomerActivityResponse>> GetCustomerActivityAsync(int inactiveDays, CancellationToken ct);
    Task<ReportCreditKpiResponse> GetCreditKpisAsync(int rangeDays, int thresholdDays, CancellationToken ct);
    Task<IReadOnlyCollection<ReportSalesByDayResponse>> GetSalesByDayAsync(int rangeDays, CancellationToken ct);
    Task<IReadOnlyCollection<ReportSalesByPaymentTypeResponse>> GetSalesByPaymentTypeAsync(int rangeDays, CancellationToken ct);
    Task<IReadOnlyCollection<ReportOverdueCustomerResponse>> GetOverdueCustomersAsync(int topN, CancellationToken ct);
}

public interface IUserService
{
    Task<PaginatedResult<UserResponse>> GetAsync(int page, int pageSize, string? search, CancellationToken ct);
    Task<UserResponse> CreateAsync(UserRequest request, CancellationToken ct);
    Task<UserResponse> UpdateAsync(Guid id, UserUpdateRequest request, CancellationToken ct);
    Task<UserResponse> UpdateStatusAsync(Guid id, bool isActive, CancellationToken ct);
    Task AssignRolesAsync(Guid id, AssignRoleRequest request, CancellationToken ct);
}

public interface IRoleService
{
    Task<IReadOnlyCollection<RoleResponse>> GetAsync(CancellationToken ct);
    Task<RoleResponse> CreateAsync(RoleRequest request, CancellationToken ct);
    Task AssignPermissionsAsync(Guid id, AssignPermissionsRequest request, CancellationToken ct);
    Task<IReadOnlyCollection<PermissionResponse>> GetPermissionsAsync(CancellationToken ct);
}

public interface IDemoSeedService
{
    Task SeedDemoAsync(string? slug = null, CancellationToken ct = default);
}
