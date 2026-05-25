using System.Text.Json;
using ClinicBackend.Api.Common;
using ClinicBackend.Api.Infrastructure;

namespace ClinicBackend.Api.MedicalFiles;

public sealed class MedicalFileService
{
    private readonly SupabaseRestClient _restClient;

    public MedicalFileService(SupabaseRestClient restClient)
    {
        _restClient = restClient;
    }

    public async Task<string> CreateMedicalFileAsync(CreateMedicalFileRequest request)
    {
        try
        {
            await _restClient.SelectSingleAsync(
                "patients",
                $"id=eq.{Uri.EscapeDataString(request.PatientId)}&select=id"
            );
        }
        catch (KeyNotFoundException)
        {
            throw new ApiException("Patient not found", StatusCodes.Status404NotFound, "NotFoundError");
        }

        var createPayload = new Dictionary<string, object?>
        {
            ["doctor_id"] = request.DoctorId,
            ["data"] = request.Data.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null
                ? new Dictionary<string, object?>()
                : ToPlainObject(request.Data),
            ["updated_at"] = DateTime.UtcNow.ToString("O")
        };

        using var createdDoc = await _restClient.InsertAsync("patient_medical_files", createPayload);
        var createdId = ExtractFirstId(createdDoc);

        await _restClient.PatchAsync(
            "patients",
            $"id=eq.{Uri.EscapeDataString(request.PatientId)}",
            new { medical_file_id = createdId },
            returnRepresentation: false
        );

        return createdId;
    }

    public async Task<object?> GetMedicalFileByPatientIdAsync(string patientId)
    {
        JsonElement patient;
        try
        {
            using var patientDoc = await _restClient.SelectSingleAsync(
                "patients",
                $"id=eq.{Uri.EscapeDataString(patientId)}&select=id,medical_file_id"
            );
            patient = patientDoc.RootElement.Clone();
        }
        catch (KeyNotFoundException)
        {
            throw new ApiException("Patient not found", StatusCodes.Status404NotFound, "NotFoundError");
        }

        if (!patient.TryGetProperty("medical_file_id", out var medicalFileIdElement) || medicalFileIdElement.ValueKind == JsonValueKind.Null)
        {
            throw new ApiException("Medical file not found", StatusCodes.Status404NotFound, "NotFoundError");
        }

        var medicalFileId = medicalFileIdElement.GetString();
        if (string.IsNullOrWhiteSpace(medicalFileId))
        {
            throw new ApiException("Medical file not found", StatusCodes.Status404NotFound, "NotFoundError");
        }

        try
        {
            using var fileDoc = await _restClient.SelectSingleAsync(
                "patient_medical_files",
                $"id=eq.{Uri.EscapeDataString(medicalFileId)}&select=id,doctor_id,data"
            );

            return JsonSerializer.Deserialize<object>(fileDoc.RootElement.GetRawText(), JsonOptions.CamelCase());
        }
        catch (KeyNotFoundException)
        {
            throw new ApiException("Medical file not found", StatusCodes.Status404NotFound, "NotFoundError");
        }
    }

    public async Task UpdateMedicalFileAsync(string id, UpdateMedicalFileRequest request)
    {
        JsonElement current;
        try
        {
            using var currentDoc = await _restClient.SelectSingleAsync(
                "patient_medical_files",
                $"id=eq.{Uri.EscapeDataString(id)}&select=data"
            );
            current = currentDoc.RootElement.Clone();
        }
        catch (KeyNotFoundException)
        {
            throw new ApiException("Medical file not found", StatusCodes.Status404NotFound, "NotFoundError");
        }

        var currentData = current.TryGetProperty("data", out var dataElement)
            ? ToDictionary(dataElement)
            : new Dictionary<string, object?>();

        if (request.Data.ValueKind != JsonValueKind.Undefined && request.Data.ValueKind != JsonValueKind.Null)
        {
            foreach (var (key, value) in ToDictionary(request.Data))
            {
                currentData[key] = value;
            }
        }

        var updatePayload = new Dictionary<string, object?>
        {
            ["data"] = currentData,
            ["updated_at"] = DateTime.UtcNow.ToString("O")
        };

        if (request.DoctorId is not null)
        {
            updatePayload["doctor_id"] = request.DoctorId;
        }

        await _restClient.PatchAsync(
            "patient_medical_files",
            $"id=eq.{Uri.EscapeDataString(id)}",
            updatePayload,
            returnRepresentation: false
        );
    }

    public Task DeleteMedicalFileAsync(string id)
    {
        return _restClient.DeleteAsync("patient_medical_files", $"id=eq.{Uri.EscapeDataString(id)}");
    }

    private static string ExtractFirstId(JsonDocument doc)
    {
        if (doc.RootElement.ValueKind == JsonValueKind.Array && doc.RootElement.GetArrayLength() > 0)
        {
            var id = doc.RootElement[0].GetProperty("id").GetString();
            if (!string.IsNullOrWhiteSpace(id))
            {
                return id;
            }
        }

        throw new ApiException("Failed to create medical file", StatusCodes.Status500InternalServerError, "DatabaseError");
    }

    private static Dictionary<string, object?> ToDictionary(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return new Dictionary<string, object?>();
        }

        var result = new Dictionary<string, object?>();
        foreach (var property in element.EnumerateObject())
        {
            result[property.Name] = ToPlainObject(property.Value);
        }

        return result;
    }

    private static object? ToPlainObject(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.Object => value.EnumerateObject().ToDictionary(p => p.Name, p => ToPlainObject(p.Value)),
            JsonValueKind.Array => value.EnumerateArray().Select(ToPlainObject).ToList(),
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number when value.TryGetInt64(out var l) => l,
            JsonValueKind.Number when value.TryGetDecimal(out var d) => d,
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => null
        };
    }
}
