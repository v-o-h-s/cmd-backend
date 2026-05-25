using System.Text.RegularExpressions;
using ClinicBackend.Api.Auth;
using ClinicBackend.Api.Common;
using ClinicBackend.Api.Patients;
using Microsoft.AspNetCore.Mvc;

namespace ClinicBackend.Api.Controllers;

[ApiController]
[Route("patients")]
[RequireAuth]
[RequireRole("admin", "receptionist")]
public sealed class PatientsController : ControllerBase
{
    private static readonly Regex BirthDateRegex = new("^\\d{4}-\\d{2}-\\d{2}$", RegexOptions.Compiled);
    private readonly PatientService _patientService;

    public PatientsController(PatientService patientService)
    {
        _patientService = patientService;
    }

    [HttpPost("add-patient")]
    public async Task<IActionResult> AddPatient([FromBody] CreatePatientRequest request)
    {
        ValidateCreateRequest(request);

        await _patientService.AddPatientAsync(request);
        return StatusCode(StatusCodes.Status201Created, ApiResponse.Ok(null, "Patient created successfully"));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPatientById([FromRoute] string id)
    {
        var data = await _patientService.GetPatientByIdAsync(id);
        return Ok(ApiResponse.Ok(data, "Patient retrieved successfully"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePatientById([FromRoute] string id)
    {
        await _patientService.DeletePatientByIdAsync(id);
        return Ok(ApiResponse.Ok(null, "Patient deleted successfully"));
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPatients()
    {
        var data = await _patientService.GetAllPatientsAsync();
        return Ok(ApiResponse.Ok(data, "Patients retrieved successfully"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePatient([FromRoute] string id, [FromBody] UpdatePatientRequest request)
    {
        ValidateUpdateRequest(request);

        var data = await _patientService.UpdatePatientAsync(id, request);
        return Ok(ApiResponse.Ok(data, "Patient updated successfully"));
    }

    private static void ValidateCreateRequest(CreatePatientRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.LastName) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.PhoneNumber) ||
            string.IsNullOrWhiteSpace(request.BirthDate) ||
            string.IsNullOrWhiteSpace(request.Gender) ||
            string.IsNullOrWhiteSpace(request.Address) ||
            string.IsNullOrWhiteSpace(request.Profession) ||
            string.IsNullOrWhiteSpace(request.FamilySituation) ||
            string.IsNullOrWhiteSpace(request.InsuranceNumber) ||
            string.IsNullOrWhiteSpace(request.EmergencyContactName) ||
            string.IsNullOrWhiteSpace(request.EmergencyContactPhone))
        {
            throw new ApiException("Validation failed", StatusCodes.Status400BadRequest, "ValidationError");
        }

        if (!BirthDateRegex.IsMatch(request.BirthDate))
        {
            throw new ApiException("Validation failed", StatusCodes.Status400BadRequest, "ValidationError");
        }

        if (request.ChildrenNumber < 0)
        {
            throw new ApiException("Validation failed", StatusCodes.Status400BadRequest, "ValidationError");
        }

        if (request.DoctorId is not null && !Guid.TryParse(request.DoctorId, out _))
        {
            throw new ApiException("Validation failed", StatusCodes.Status400BadRequest, "ValidationError");
        }
    }

    private static void ValidateUpdateRequest(UpdatePatientRequest request)
    {
        if (request.ChildrenNumber.HasValue && request.ChildrenNumber.Value < 0)
        {
            throw new ApiException("Validation failed", StatusCodes.Status400BadRequest, "ValidationError");
        }

        if (request.BirthDate is not null && !BirthDateRegex.IsMatch(request.BirthDate))
        {
            throw new ApiException("Validation failed", StatusCodes.Status400BadRequest, "ValidationError");
        }

        if (request.MedicalFileId is not null && !Guid.TryParse(request.MedicalFileId, out _))
        {
            throw new ApiException("Validation failed", StatusCodes.Status400BadRequest, "ValidationError");
        }
    }
}
