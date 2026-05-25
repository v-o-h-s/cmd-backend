using System.Text.Json;
using ClinicBackend.Api.Common;
using ClinicBackend.Api.Infrastructure;

namespace ClinicBackend.Api.Staff;

public sealed class ReceptionistService
{
    private readonly SupabaseRestClient _restClient;

    public ReceptionistService(SupabaseRestClient restClient)
    {
        _restClient = restClient;
    }

    public async Task<object> GetReceptionistsAsync(int page, int limit)
    {
        var offset = (page - 1) * limit;
        var query =
            "select=id,phone_number,profiles!inner(first_name,last_name,email)" +
            $"&offset={offset}&limit={limit}";

        using var dataDoc = await _restClient.SelectManyAsync("receptionists", query);
        using var countDoc = await _restClient.SelectManyAsync("receptionists", "select=id");

        var receptionists = dataDoc.RootElement.EnumerateArray().Select(MapReceptionist).ToList();
        var total = countDoc.RootElement.GetArrayLength();

        return new
        {
            total,
            receptionists
        };
    }

    public async Task<object> GetReceptionistByIdAsync(string id)
    {
        var query =
            "select=id,phone_number,profiles!inner(first_name,last_name,email,role)" +
            $"&id=eq.{Uri.EscapeDataString(id)}";

        try
        {
            using var doc = await _restClient.SelectSingleAsync("receptionists", query);
            return MapReceptionist(doc.RootElement);
        }
        catch (KeyNotFoundException)
        {
            throw new ApiException("Receptionist not found", StatusCodes.Status404NotFound, "NotFoundError");
        }
    }

    public async Task<object> UpdateReceptionistByIdAsync(string id, JsonElement body)
    {
        var profilePatch = new Dictionary<string, object?>();
        var receptionistPatch = new Dictionary<string, object?>();

        if (body.TryGetProperty("firstName", out var firstName)) profilePatch["first_name"] = firstName.GetString();
        if (body.TryGetProperty("lastName", out var lastName)) profilePatch["last_name"] = lastName.GetString();
        if (body.TryGetProperty("email", out var email)) profilePatch["email"] = email.GetString();
        if (body.TryGetProperty("phoneNumber", out var phoneNumber)) receptionistPatch["phone_number"] = phoneNumber.GetString();
        if (body.TryGetProperty("phone_number", out var phoneNumberDb)) receptionistPatch["phone_number"] = phoneNumberDb.GetString();

        if (profilePatch.Count > 0)
        {
            await _restClient.PatchAsync("profiles", $"id=eq.{Uri.EscapeDataString(id)}", profilePatch, returnRepresentation: false);
        }

        if (receptionistPatch.Count > 0)
        {
            await _restClient.PatchAsync("receptionists", $"id=eq.{Uri.EscapeDataString(id)}", receptionistPatch, returnRepresentation: false);
        }

        return await GetReceptionistByIdAsync(id);
    }

    public async Task DeleteReceptionistByIdAsync(string id)
    {
        await _restClient.DeleteAsync("receptionists", $"id=eq.{Uri.EscapeDataString(id)}");
    }

    private static object MapReceptionist(JsonElement raw)
    {
        var profile = raw.GetProperty("profiles");

        return new
        {
            id = raw.GetProperty("id").GetString(),
            first_name = profile.GetProperty("first_name").GetString(),
            last_name = profile.GetProperty("last_name").GetString(),
            email = profile.GetProperty("email").GetString(),
            phone_number = raw.TryGetProperty("phone_number", out var phone) && phone.ValueKind != JsonValueKind.Null ? phone.GetString() : null
        };
    }
}
