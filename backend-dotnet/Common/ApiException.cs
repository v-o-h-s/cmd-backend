namespace ClinicBackend.Api.Common;

public sealed class ApiException : Exception
{
    public int StatusCode { get; }
    public string ErrorType { get; }
    public object? Context { get; }

    public ApiException(string message, int statusCode, string errorType, object? context = null) : base(message)
    {
        StatusCode = statusCode;
        ErrorType = errorType;
        Context = context;
    }
}
