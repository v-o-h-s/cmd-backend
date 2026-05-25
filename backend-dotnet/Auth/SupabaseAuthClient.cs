using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ClinicBackend.Api.Common;
using ClinicBackend.Api.Infrastructure;
using Microsoft.Extensions.Options;

namespace ClinicBackend.Api.Auth;

public sealed class SupabaseAuthClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly SupabaseOptions _options;

    public SupabaseAuthClient(IHttpClientFactory httpClientFactory, IOptions<SupabaseOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    public async Task<AuthTokenResponse> LoginAsync(string email, string password)
    {
        return await PostTokenAsync("password", new
        {
            email,
            password
        }, "Invalid email or password", StatusCodes.Status400BadRequest, "ValidationError");
    }

    public async Task<AuthTokenResponse> RefreshAsync(string refreshToken)
    {
        return await PostTokenAsync("refresh_token", new
        {
            refresh_token = refreshToken
        }, "Invalid or expired refresh token", StatusCodes.Status401Unauthorized, "AuthenticationError");
    }

    public async Task<JsonDocument> GetUserFromAccessTokenAsync(string accessToken)
    {
        var client = CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "auth/v1/user");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.TryAddWithoutValidation("apikey", _options.PublishableDefaultKey);

        var response = await client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new ApiException("Invalid or expired token", StatusCodes.Status401Unauthorized, "AuthenticationError", body);
        }

        return JsonDocument.Parse(body);
    }

    private async Task<AuthTokenResponse> PostTokenAsync(
        string grantType,
        object payload,
        string errorMessage,
        int statusCode,
        string errorType)
    {
        var client = CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, $"auth/v1/token?grant_type={grantType}");
        request.Headers.TryAddWithoutValidation("apikey", _options.PublishableDefaultKey);
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new ApiException(errorMessage, statusCode, errorType, body);
        }

        var data = JsonSerializer.Deserialize<AuthTokenResponse>(body, JsonOptions.CamelCase())
                   ?? throw new ApiException("Authentication response parsing failed", StatusCodes.Status500InternalServerError, "InternalServerError");

        return data;
    }

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(_options.Url.TrimEnd('/') + "/");
        client.DefaultRequestHeaders.Clear();
        return client;
    }
}
