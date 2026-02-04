namespace Perfect.Application.Models;

public record PaginatedResult<T>(IReadOnlyCollection<T> Items, int Page, int PageSize, int TotalItems)
{
    public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
}

public record ApiResponse<T>(T Data, ApiMeta? Meta = null, IReadOnlyCollection<ApiError>? Errors = null);

public record ApiMeta(int Page, int PageSize, int TotalItems, int TotalPages);

public record ApiError(string Code, string Message);
