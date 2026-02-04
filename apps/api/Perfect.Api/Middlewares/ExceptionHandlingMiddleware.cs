using System.Text.Json;
using Perfect.Application.Common;

namespace Perfect.Api.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var correlationId = context.Response.Headers["X-Correlation-Id"].FirstOrDefault()
            ?? context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
            ?? context.TraceIdentifier;

        try
        {
            await _next(context);
        }
        catch (AppException ex)
        {
            _logger.LogWarning(ex, "Application error: {Code}", ex.Code);
            context.Response.StatusCode = ex.StatusCode;
            context.Response.ContentType = "application/problem+json";
            var payload = new
            {
                type = $"https://perfect.app/errors/{ex.Code}",
                title = ex.Message,
                status = ex.StatusCode,
                detail = ex.Message,
                code = ex.Code,
                correlationId
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/problem+json";
            var payload = new
            {
                type = "https://perfect.app/errors/internal",
                title = "Unexpected error",
                status = 500,
                detail = "An unexpected error occurred",
                code = "internal_error",
                correlationId
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
    }
}
