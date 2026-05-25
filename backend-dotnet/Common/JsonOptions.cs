using System.Text.Json;

namespace ClinicBackend.Api.Common;

public static class JsonOptions
{
    public static JsonSerializerOptions CamelCase() => new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}
