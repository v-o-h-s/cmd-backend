using System.Text.Json;
using ClinicBackend.Api.Auth;
using ClinicBackend.Api.Common;
using ClinicBackend.Api.Staff;
using Microsoft.AspNetCore.Mvc;

namespace ClinicBackend.Api.Controllers;

[ApiController]
[Route("receptionists")]
[RequireAuth]
[RequireRole("admin")]
public sealed class ReceptionistsController : ControllerBase
{
    private readonly ReceptionistService _receptionistService;

    public ReceptionistsController(ReceptionistService receptionistService)
    {
        _receptionistService = receptionistService;
    }

    [HttpGet]
    public async Task<IActionResult> GetReceptionists([FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        var data = await _receptionistService.GetReceptionistsAsync(page <= 0 ? 1 : page, limit <= 0 ? 10 : limit);
        return Ok(ApiResponse.Ok(data, "Receptionists retrieved successfully"));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetReceptionistById([FromRoute] string id)
    {
        var data = await _receptionistService.GetReceptionistByIdAsync(id);
        return Ok(ApiResponse.Ok(data, "Receptionist retrieved successfully"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateReceptionistById([FromRoute] string id, [FromBody] JsonElement body)
    {
        var data = await _receptionistService.UpdateReceptionistByIdAsync(id, body);
        return Ok(ApiResponse.Ok(data, "Receptionist updated successfully"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReceptionistById([FromRoute] string id)
    {
        await _receptionistService.DeleteReceptionistByIdAsync(id);
        return Ok(ApiResponse.Ok(null, "Receptionist deleted successfully"));
    }
}
