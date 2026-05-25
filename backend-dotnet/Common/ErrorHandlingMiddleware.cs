using System.Text.Json;

namespace ClinicBackend.Api.Common;

public sealed class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ApiException ex)
        {
            context.Response.StatusCode = ex.StatusCode;
            context.Response.ContentType = "application/json";

            var payload = ApiResponse.Fail(ex.Message, new
            {
                type = ex.ErrorType,
                context = ex.Context,
                message = ex.Message
            });

            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            var payload = ApiResponse.Fail(ex.Message, new
            {
                type = "InternalServerError",
                message = ex.Message
            });

            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
    }
}
