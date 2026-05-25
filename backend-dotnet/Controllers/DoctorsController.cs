using System.Text.Json;
using ClinicBackend.Api.Auth;
using ClinicBackend.Api.Common;
using ClinicBackend.Api.Staff;
using Microsoft.AspNetCore.Mvc;

namespace ClinicBackend.Api.Controllers;

[ApiController]
[Route("doctors")]
[RequireAuth]
[RequireRole("admin")]
public sealed class DoctorsController : ControllerBase
{
    private readonly DoctorService _doctorService;

    public DoctorsController(DoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    [HttpGet]
    public async Task<IActionResult> GetDoctors([FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        var data = await _doctorService.GetDoctorsAsync(page <= 0 ? 1 : page, limit <= 0 ? 10 : limit);
        return Ok(ApiResponse.Ok(data, "Doctors retrieved successfully"));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDoctorById([FromRoute] string id)
    {
        var data = await _doctorService.GetDoctorByIdAsync(id);
        return Ok(ApiResponse.Ok(data, "Doctor retrieved successfully"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDoctorById([FromRoute] string id, [FromBody] JsonElement body)
    {
        var data = await _doctorService.UpdateDoctorByIdAsync(id, body);
        return Ok(ApiResponse.Ok(data, "Doctor updated successfully"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDoctorById([FromRoute] string id)
    {
        await _doctorService.DeleteDoctorByIdAsync(id);
        return Ok(ApiResponse.Ok(null, "Doctor deleted successfully"));
    }
}
