namespace Perfect.Application.Common;

public class AppException : Exception
{
    public string Code { get; }
    public int StatusCode { get; }

    public AppException(string code, string message, int statusCode = 400) : base(message)
    {
        Code = code;
        StatusCode = statusCode;
    }
}

public static class ErrorCodes
{
    public const string NotFound = "not_found";
    public const string Validation = "validation_error";
    public const string Unauthorized = "unauthorized";
    public const string Forbidden = "forbidden";
    public const string Conflict = "conflict";
    public const string TenantMissing = "tenant_missing";
}
