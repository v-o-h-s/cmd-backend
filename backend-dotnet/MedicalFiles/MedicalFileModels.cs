using System.Text.Json;

namespace ClinicBackend.Api.MedicalFiles;

public sealed class CreateMedicalFileRequest
{
    public string PatientId { get; set; } = string.Empty;
    public string? DoctorId { get; set; }
    public JsonElement Data { get; set; }
}

public sealed class UpdateMedicalFileRequest
{
    public string? DoctorId { get; set; }
    public JsonElement Data { get; set; }
}
