using Perfect.Domain.Enums;

namespace Perfect.Application.Contracts;

public record ModuleCatalogRequest(string Name, string Slug, string? BaseUrl, string LaunchUrl, string Status, string? Icon);
public record ModuleCatalogResponse(Guid Id, string Name, string Slug, string BaseUrl, string LaunchUrl, string? Icon, string Status, DateTime CreatedAt);

public record PlatformTenantResponse(Guid Id, string Name, string? DisplayName, string Slug, string Status, string Plan, DateTime CreatedAt);
public record TenantStatusUpdateRequest(string Status);

public record ModuleAssignmentResponse(Guid ModuleId, string Name, string Slug, string BaseUrl, string LaunchUrl, string Status, bool Enabled, DateTime? ActivatedAt, string? Notes);
public record ModuleAssignmentItem(Guid ModuleId, bool Enabled, string? Notes);
public record ModuleAssignmentUpdateRequest(Guid TenantId, IReadOnlyCollection<ModuleAssignmentItem> Modules);

public record WorkspaceAppResponse(Guid ModuleId, string Name, string Slug, string BaseUrl, string LaunchUrl, string Status, bool Enabled);
public record WorkspaceUserResponse(Guid Id, string Name, string Email, IReadOnlyCollection<string> Roles, bool IsActive);
