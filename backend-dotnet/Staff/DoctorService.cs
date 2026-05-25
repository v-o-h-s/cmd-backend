using System.Text.Json;
using ClinicBackend.Api.Common;
using ClinicBackend.Api.Infrastructure;

namespace ClinicBackend.Api.Staff;

public sealed class DoctorService
{
    private readonly SupabaseRestClient _restClient;

    public DoctorService(SupabaseRestClient restClient)
    {
        _restClient = restClient;
    }

    public async Task<object> GetDoctorsAsync(int page, int limit)
    {
        var offset = (page - 1) * limit;
        var query =
            "select=id,specialization,is_medical_director,salary,phone_number,profiles!inner(first_name,last_name,email)" +
            $"&offset={offset}&limit={limit}";

        using var dataDoc = await _restClient.SelectManyAsync("doctors", query);
        using var countDoc = await _restClient.SelectManyAsync("doctors", "select=id");

        var doctors = dataDoc.RootElement.EnumerateArray().Select(MapDoctor).ToList();
        var total = countDoc.RootElement.GetArrayLength();

        return new
        {
            total,
            doctors
        };
    }

    public async Task<object> GetDoctorByIdAsync(string id)
    {
        var query =
            "select=id,specialization,is_medical_director,salary,phone_number,profiles!inner(first_name,last_name,email,role)" +
            $"&id=eq.{Uri.EscapeDataString(id)}";

        try
        {
            using var doc = await _restClient.SelectSingleAsync("doctors", query);
            return MapDoctor(doc.RootElement);
        }
        catch (KeyNotFoundException)
        {
            throw new ApiException("Doctor not found", StatusCodes.Status404NotFound, "NotFoundError");
        }
    }

    public async Task<object> UpdateDoctorByIdAsync(string id, JsonElement body)
    {
        var profilePatch = new Dictionary<string, object?>();
        var doctorPatch = new Dictionary<string, object?>();

        if (body.TryGetProperty("firstName", out var firstName)) profilePatch["first_name"] = firstName.GetString();
        if (body.TryGetProperty("lastName", out var lastName)) profilePatch["last_name"] = lastName.GetString();
        if (body.TryGetProperty("email", out var email)) profilePatch["email"] = email.GetString();
        if (body.TryGetProperty("role", out var role)) profilePatch["role"] = role.GetString()?.ToLowerInvariant();

        if (body.TryGetProperty("salary", out var salary) && salary.ValueKind != JsonValueKind.Null) doctorPatch["salary"] = salary.GetDecimal();
        if (body.TryGetProperty("specialization", out var specialization)) doctorPatch["specialization"] = specialization.GetString();
        if (body.TryGetProperty("isMedicalDirector", out var isMedicalDirector) && isMedicalDirector.ValueKind != JsonValueKind.Null) doctorPatch["is_medical_director"] = isMedicalDirector.GetBoolean();
        if (body.TryGetProperty("is_medical_director", out var isMedicalDirectorDb) && isMedicalDirectorDb.ValueKind != JsonValueKind.Null) doctorPatch["is_medical_director"] = isMedicalDirectorDb.GetBoolean();
        if (body.TryGetProperty("phoneNumber", out var phoneNumber)) doctorPatch["phone_number"] = phoneNumber.GetString();
        if (body.TryGetProperty("phone_number", out var phoneNumberDb)) doctorPatch["phone_number"] = phoneNumberDb.GetString();

        if (profilePatch.Count > 0)
        {
            await _restClient.PatchAsync("profiles", $"id=eq.{Uri.EscapeDataString(id)}", profilePatch, returnRepresentation: false);
        }

        if (doctorPatch.Count > 0)
        {
            await _restClient.PatchAsync("doctors", $"id=eq.{Uri.EscapeDataString(id)}", doctorPatch, returnRepresentation: false);
        }

        return await GetDoctorByIdAsync(id);
    }

    public async Task DeleteDoctorByIdAsync(string id)
    {
        await _restClient.DeleteAsync("doctors", $"id=eq.{Uri.EscapeDataString(id)}");
    }

    private static object MapDoctor(JsonElement raw)
    {
        var profile = raw.GetProperty("profiles");

        return new
        {
            id = raw.GetProperty("id").GetString(),
            first_name = profile.GetProperty("first_name").GetString(),
            last_name = profile.GetProperty("last_name").GetString(),
            email = profile.GetProperty("email").GetString(),
            specialization = raw.TryGetProperty("specialization", out var specialization) && specialization.ValueKind != JsonValueKind.Null ? specialization.GetString() : null,
            salary = raw.TryGetProperty("salary", out var salary) && salary.ValueKind != JsonValueKind.Null ? salary.GetDecimal() : (decimal?)null,
            phone_number = raw.TryGetProperty("phone_number", out var phone) && phone.ValueKind != JsonValueKind.Null ? phone.GetString() : null,
            is_medical_director = raw.TryGetProperty("is_medical_director", out var isMd) && isMd.ValueKind != JsonValueKind.Null && isMd.GetBoolean()
        };
    }
}
