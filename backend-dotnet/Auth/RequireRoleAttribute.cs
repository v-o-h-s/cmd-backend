using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ClinicBackend.Api.Auth;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireRoleAttribute : Attribute, IAsyncActionFilter
{
    private readonly HashSet<string> _roles;

    public RequireRoleAttribute(params string[] roles)
    {
        _roles = roles.Select(r => r.Trim().ToLowerInvariant()).ToHashSet();
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var user = context.HttpContext.GetAuthenticatedUser();
        if (user is null)
        {
            context.Result = new JsonResult(new
            {
                success = false,
                status = 401,
                data = (object?)null,
                error = new { message = "User not authenticated" }
            })
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
            return;
        }

        if (!_roles.Contains(user.Role.ToLowerInvariant()))
        {
            context.Result = new JsonResult(new
            {
                success = false,
                status = 403,
                data = (object?)null,
                error = new { message = $"Access denied. Required roles: {string.Join(", ", _roles)}" }
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
            return;
        }

        await next();
    }
}
