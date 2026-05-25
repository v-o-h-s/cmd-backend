namespace ClinicBackend.Api.Patients;

public sealed class CreatePatientRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string BirthDate { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Profession { get; set; } = string.Empty;
    public int ChildrenNumber { get; set; }
    public string FamilySituation { get; set; } = string.Empty;
    public string InsuranceNumber { get; set; } = string.Empty;
    public string EmergencyContactName { get; set; } = string.Empty;
    public string EmergencyContactPhone { get; set; } = string.Empty;
    public string? DoctorId { get; set; }
}

public sealed class UpdatePatientRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? BirthDate { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }
    public string? Profession { get; set; }
    public int? ChildrenNumber { get; set; }
    public string? FamilySituation { get; set; }
    public string? InsuranceNumber { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? MedicalFileId { get; set; }
}
