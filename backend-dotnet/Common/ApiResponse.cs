namespace ClinicBackend.Api.Common;

public sealed record ApiResponse(bool Success, string Message, object? Data, object? Error)
{
    public static ApiResponse Ok(object? data, string message) => new(true, message, data, null);

    public static ApiResponse Fail(string message, object error) => new(false, message, null, error);
}
