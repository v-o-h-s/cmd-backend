using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ClinicBackend.Api.Common;
using ClinicBackend.Api.Infrastructure;
using Microsoft.Extensions.Options;

namespace ClinicBackend.Api.Users;

public sealed class UserService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly SupabaseOptions _options;
    private readonly SupabaseRestClient _restClient;

    public UserService(IHttpClientFactory httpClientFactory, IOptions<SupabaseOptions> options, SupabaseRestClient restClient)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _restClient = restClient;
    }

    public async Task<ProfileRecord> CreateUserAsync(CreateUserRequest request)
    {
        if (request.Role is not ("doctor" or "receptionist"))
        {
            throw new ApiException("Role must be doctor or receptionist", StatusCodes.Status400BadRequest, "ValidationError");
        }

        var authUserId = await CreateAuthUserAsync(request);

        await _restClient.InsertAsync("profiles", new
        {
            id = authUserId,
            email = request.Email,
            first_name = request.FirstName,
            last_name = request.LastName,
            role = request.Role
        }, returnRepresentation: false);

        if (request.Role == "doctor")
        {
            await _restClient.InsertAsync("doctors", new { id = authUserId }, returnRepresentation: false);
        }
        else
        {
            await _restClient.InsertAsync("receptionists", new { id = authUserId }, returnRepresentation: false);
        }

        var profileDoc = await _restClient.SelectSingleAsync(
            "profiles",
            $"id=eq.{Uri.EscapeDataString(authUserId)}&select=id,email,first_name,last_name,role"
        );

        return JsonSerializer.Deserialize<ProfileRecord>(profileDoc.RootElement.GetRawText(), JsonOptions.CamelCase())
               ?? throw new ApiException("Failed to load created user", StatusCodes.Status500InternalServerError, "InternalServerError");
    }

    private async Task<string> CreateAuthUserAsync(CreateUserRequest request)
    {
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(_options.Url.TrimEnd('/') + "/");

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "auth/v1/admin/users");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ServiceKey);
        httpRequest.Headers.TryAddWithoutValidation("apikey", _options.ServiceKey);

        httpRequest.Content = new StringContent(JsonSerializer.Serialize(new
        {
            email = request.Email,
            password = request.Password,
            email_confirm = true,
            user_metadata = new
            {
                role = request.Role
            }
        }), Encoding.UTF8, "application/json");

        var response = await client.SendAsync(httpRequest);
        var text = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new ApiException("User creation failed", StatusCodes.Status400BadRequest, "DatabaseError", text);
        }

        using var doc = JsonDocument.Parse(text);
        var id = doc.RootElement.GetProperty("id").GetString();
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ApiException("User creation failed", StatusCodes.Status500InternalServerError, "DatabaseError");
        }

        return id;
    }
}
