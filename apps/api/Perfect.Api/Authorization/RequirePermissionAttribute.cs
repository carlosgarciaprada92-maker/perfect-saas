using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Perfect.Api.Authorization;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class RequirePermissionAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string _permission;

    public RequirePermissionAttribute(string permission)
    {
        _permission = permission;
    }

    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (user.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedResult();
            return Task.CompletedTask;
        }

        var hasPermission = user.Claims.Any(c => c.Type == "perm" && c.Value == _permission)
            || user.IsInRole("TenantOwner")
            || user.IsInRole("ADMIN");
        if (!hasPermission)
        {
            context.Result = new ObjectResult(new { code = "forbidden", message = $"Missing permission {_permission}" }) { StatusCode = 403 };
        }

        return Task.CompletedTask;
    }
}
