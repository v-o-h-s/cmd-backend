using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ClinicBackend.Api.Auth;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireAuthAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var authService = context.HttpContext.RequestServices.GetRequiredService<AuthService>();

        var token = context.HttpContext.Request.Cookies["accessToken"];
        if (string.IsNullOrWhiteSpace(token))
        {
            var authHeader = context.HttpContext.Request.Headers.Authorization.ToString();
            if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = authHeader[7..].Trim();
            }
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            context.Result = new JsonResult(new
            {
                success = false,
                message = "No access token provided in cookies or authorization header",
                data = (object?)null,
                error = new { type = "AuthenticationError", message = "No access token provided in cookies or authorization header" }
            })
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
            return;
        }

        var profile = await authService.GetProfileFromTokenAsync(token);
        context.HttpContext.Items[AuthHttpContextExtensions.UserItemKey] = new AuthenticatedUser
        {
            Id = profile.Id,
            Email = profile.Email,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            Role = profile.Role,
            Token = token
        };

        await next();
    }
}
