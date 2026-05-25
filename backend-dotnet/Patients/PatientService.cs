using System.Text.Json;
using ClinicBackend.Api.Common;
using ClinicBackend.Api.Infrastructure;
using ClinicBackend.Api.MedicalFiles;

namespace ClinicBackend.Api.Patients;

public sealed class PatientService
{
    private readonly SupabaseRestClient _restClient;
    private readonly MedicalFileService _medicalFileService;

    public PatientService(SupabaseRestClient restClient, MedicalFileService medicalFileService)
    {
        _restClient = restClient;
        _medicalFileService = medicalFileService;
    }

    public async Task AddPatientAsync(CreatePatientRequest request)
    {
        var id = Guid.NewGuid().ToString();

        var payload = new Dictionary<string, object?>
        {
            ["id"] = id,
            ["first_name"] = request.FirstName,
            ["last_name"] = request.LastName,
            ["email"] = request.Email,
            ["phone_number"] = request.PhoneNumber,
            ["birth_date"] = request.BirthDate,
            ["gender"] = request.Gender,
            ["address"] = request.Address,
            ["profession"] = request.Profession,
            ["children_number"] = request.ChildrenNumber,
            ["family_situation"] = request.FamilySituation,
            ["insurance_number"] = request.InsuranceNumber,
            ["emergency_contact_name"] = request.EmergencyContactName,
            ["emergency_contact_phone"] = request.EmergencyContactPhone,
            ["medical_file_id"] = null
        };

        await _restClient.InsertAsync("patients", payload, returnRepresentation: false);

        await _medicalFileService.CreateMedicalFileAsync(new CreateMedicalFileRequest
        {
            PatientId = id,
            DoctorId = request.DoctorId
        });
    }

    public async Task<object> GetPatientByIdAsync(string id)
    {
        JsonElement patient;
        try
        {
            using var doc = await _restClient.SelectSingleAsync("patients", $"id=eq.{Uri.EscapeDataString(id)}&select=*");
            patient = doc.RootElement.Clone();
        }
        catch (KeyNotFoundException)
        {
            throw new ApiException("Patient not found", StatusCodes.Status404NotFound, "NotFoundError");
        }

        return new
        {
            id = GetString(patient, "id"),
            firstName = GetString(patient, "first_name"),
            lastName = GetString(patient, "last_name"),
            email = GetString(patient, "email"),
            phoneNumber = GetString(patient, "phone_number"),
            birthDate = GetString(patient, "birth_date"),
            gender = GetString(patient, "gender"),
            address = GetString(patient, "address"),
            profession = GetString(patient, "profession"),
            childrenNumber = GetInt(patient, "children_number") ?? 0,
            familySituation = GetString(patient, "family_situation"),
            insuranceNumber = GetString(patient, "insurance_number"),
            emergencyContactName = GetString(patient, "emergency_contact_name"),
            emergencyContactPhone = GetString(patient, "emergency_contact_phone"),
            medicalFileId = GetStringOrNull(patient, "medical_file_id")
        };
    }

    public async Task DeletePatientByIdAsync(string id)
    {
        await _restClient.DeleteAsync("patients", $"id=eq.{Uri.EscapeDataString(id)}");
    }

    public async Task<object> GetAllPatientsAsync()
    {
        using var doc = await _restClient.SelectManyAsync("patients", "select=*");

        var data = doc.RootElement.EnumerateArray().Select(patient => new
        {
            id = GetString(patient, "id"),
            firstName = GetString(patient, "first_name"),
            lastName = GetString(patient, "last_name"),
            email = GetString(patient, "email"),
            phoneNumber = GetString(patient, "phone_number"),
            age = ComputeAge(GetString(patient, "birth_date")),
            gender = GetString(patient, "gender")
        }).ToList();

        return data;
    }

    public async Task<object> UpdatePatientAsync(string id, UpdatePatientRequest request)
    {
        JsonElement current;
        try
        {
            using var doc = await _restClient.SelectSingleAsync("patients", $"id=eq.{Uri.EscapeDataString(id)}&select=*");
            current = doc.RootElement.Clone();
        }
        catch (KeyNotFoundException)
        {
            throw new ApiException("Patient not found", StatusCodes.Status404NotFound, "NotFoundError");
        }

        var payload = new Dictionary<string, object?>
        {
            ["first_name"] = request.FirstName ?? GetString(current, "first_name"),
            ["last_name"] = request.LastName ?? GetString(current, "last_name"),
            ["email"] = request.Email ?? GetString(current, "email"),
            ["phone_number"] = request.PhoneNumber ?? GetString(current, "phone_number"),
            ["birth_date"] = request.BirthDate ?? GetString(current, "birth_date"),
            ["gender"] = request.Gender ?? GetString(current, "gender"),
            ["address"] = request.Address ?? GetString(current, "address"),
            ["profession"] = request.Profession ?? GetString(current, "profession"),
            ["children_number"] = request.ChildrenNumber ?? GetInt(current, "children_number") ?? 0,
            ["family_situation"] = request.FamilySituation ?? GetString(current, "family_situation"),
            ["insurance_number"] = request.InsuranceNumber ?? GetString(current, "insurance_number"),
            ["emergency_contact_name"] = request.EmergencyContactName ?? GetString(current, "emergency_contact_name"),
            ["emergency_contact_phone"] = request.EmergencyContactPhone ?? GetString(current, "emergency_contact_phone"),
            ["medical_file_id"] = request.MedicalFileId ?? GetStringOrNull(current, "medical_file_id")
        };

        await _restClient.PatchAsync("patients", $"id=eq.{Uri.EscapeDataString(id)}", payload, returnRepresentation: false);

        return await GetPatientByIdAsync(id);
    }

    private static string GetString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return string.Empty;
        }

        return value.GetString() ?? string.Empty;
    }

    private static string? GetStringOrNull(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return value.GetString();
    }

    private static int? GetInt(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var intValue))
        {
            return intValue;
        }

        if (value.ValueKind == JsonValueKind.String && int.TryParse(value.GetString(), out var parsed))
        {
            return parsed;
        }

        return null;
    }

    private static int ComputeAge(string birthDate)
    {
        if (!DateOnly.TryParse(birthDate, out var dob))
        {
            return 0;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - dob.Year;
        if (dob > today.AddYears(-age))
        {
            age--;
        }

        return age;
    }
}
