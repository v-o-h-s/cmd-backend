using ClinicBackend.Api.Auth;
using ClinicBackend.Api.Common;
using ClinicBackend.Api.MedicalFiles;
using Microsoft.AspNetCore.Mvc;

namespace ClinicBackend.Api.Controllers;

[ApiController]
[Route("medical-files")]
[RequireAuth]
public sealed class MedicalFilesController : ControllerBase
{
    private readonly MedicalFileService _medicalFileService;

    public MedicalFilesController(MedicalFileService medicalFileService)
    {
        _medicalFileService = medicalFileService;
    }

    [HttpPost]
    [RequireRole("doctor", "admin")]
    public async Task<IActionResult> CreateMedicalFile([FromBody] CreateMedicalFileRequest request)
    {
        if (!Guid.TryParse(request.PatientId, out _))
        {
            throw new ApiException("Validation failed", StatusCodes.Status400BadRequest, "ValidationError");
        }

        if (request.DoctorId is not null && !Guid.TryParse(request.DoctorId, out _))
        {
            throw new ApiException("Validation failed", StatusCodes.Status400BadRequest, "ValidationError");
        }

        await _medicalFileService.CreateMedicalFileAsync(request);
        return StatusCode(StatusCodes.Status201Created, ApiResponse.Ok(null, "Medical file created successfully"));
    }

    [HttpGet("patient/{patientId}")]
    [RequireRole("doctor", "admin", "receptionist")]
    public async Task<IActionResult> GetMedicalFileByPatientId([FromRoute] string patientId)
    {
        var data = await _medicalFileService.GetMedicalFileByPatientIdAsync(patientId);
        return Ok(ApiResponse.Ok(data, "Medical file retrieved successfully"));
    }

    [HttpPut("{id}")]
    [RequireRole("doctor", "admin")]
    public async Task<IActionResult> UpdateMedicalFile([FromRoute] string id, [FromBody] UpdateMedicalFileRequest request)
    {
        await _medicalFileService.UpdateMedicalFileAsync(id, request);
        return Ok(ApiResponse.Ok(null, "Medical file updated successfully"));
    }

    [HttpDelete("{id}")]
    [RequireRole("doctor", "admin")]
    public async Task<IActionResult> DeleteMedicalFile([FromRoute] string id)
    {
        await _medicalFileService.DeleteMedicalFileAsync(id);
        return Ok(ApiResponse.Ok(null, "Medical file deleted successfully"));
    }
}
