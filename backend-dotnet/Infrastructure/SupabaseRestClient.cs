using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace ClinicBackend.Api.Infrastructure;

public sealed class SupabaseRestClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly SupabaseOptions _options;

    public SupabaseRestClient(IHttpClientFactory httpClientFactory, IOptions<SupabaseOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    public async Task<JsonDocument> SelectSingleAsync(string table, string query, bool useServiceKey = true)
    {
        var client = CreateClient(useServiceKey);
        var url = BuildRestUrl(table, query + "&limit=1");
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.TryAddWithoutValidation("Accept", "application/json");

        var response = await client.SendAsync(request);
        var text = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(text);
        }

        var doc = JsonDocument.Parse(text);
        if (doc.RootElement.ValueKind != JsonValueKind.Array || doc.RootElement.GetArrayLength() == 0)
        {
            throw new KeyNotFoundException("Record not found");
        }

        var elementText = doc.RootElement[0].GetRawText();
        return JsonDocument.Parse(elementText);
    }

    public async Task<JsonDocument> SelectManyAsync(string table, string query, bool useServiceKey = true)
    {
        var client = CreateClient(useServiceKey);
        var url = BuildRestUrl(table, query);
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.TryAddWithoutValidation("Accept", "application/json");

        var response = await client.SendAsync(request);
        var text = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(text);
        }

        return JsonDocument.Parse(text);
    }

    public async Task<JsonDocument> InsertAsync(string table, object payload, bool returnRepresentation = true, bool useServiceKey = true)
    {
        var client = CreateClient(useServiceKey);
        var url = BuildRestUrl(table, string.Empty);
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        request.Headers.TryAddWithoutValidation("Prefer", returnRepresentation ? "return=representation" : "return=minimal");

        var response = await client.SendAsync(request);
        var text = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(text);
        }

        return JsonDocument.Parse(text);
    }

    public async Task<JsonDocument> PatchAsync(string table, string query, object payload, bool returnRepresentation = true, bool useServiceKey = true)
    {
        var client = CreateClient(useServiceKey);
        var url = BuildRestUrl(table, query);
        using var request = new HttpRequestMessage(HttpMethod.Patch, url);
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        request.Headers.TryAddWithoutValidation("Prefer", returnRepresentation ? "return=representation" : "return=minimal");

        var response = await client.SendAsync(request);
        var text = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(text);
        }

        return JsonDocument.Parse(text);
    }

    public async Task DeleteAsync(string table, string query, bool useServiceKey = true)
    {
        var client = CreateClient(useServiceKey);
        var url = BuildRestUrl(table, query);
        using var request = new HttpRequestMessage(HttpMethod.Delete, url);

        var response = await client.SendAsync(request);
        var text = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(text);
        }
    }

    private HttpClient CreateClient(bool useServiceKey)
    {
        var client = _httpClientFactory.CreateClient();
        var key = useServiceKey ? _options.ServiceKey : _options.PublishableDefaultKey;
        client.BaseAddress = new Uri(_options.Url.TrimEnd('/') + "/");
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);
        client.DefaultRequestHeaders.TryAddWithoutValidation("apikey", key);
        return client;
    }

    private string BuildRestUrl(string table, string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return $"rest/v1/{table}";
        }

        return $"rest/v1/{table}?{query}";
    }
}
