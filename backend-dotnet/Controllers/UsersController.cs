using ClinicBackend.Api.Auth;
using ClinicBackend.Api.Common;
using ClinicBackend.Api.Users;
using Microsoft.AspNetCore.Mvc;

namespace ClinicBackend.Api.Controllers;

[ApiController]
[Route("users")]
public sealed class UsersController : ControllerBase
{
    private readonly UserService _userService;

    public UsersController(UserService userService)
    {
        _userService = userService;
    }

    [HttpPost("add-user")]
    [RequireAuth]
    [RequireRole("admin")]
    public async Task<IActionResult> AddUser([FromBody] CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName) ||
            request.Password.Length < 2)
        {
            throw new ApiException("Validation failed", StatusCodes.Status400BadRequest, "ValidationError");
        }

        var user = await _userService.CreateUserAsync(request);

        return StatusCode(StatusCodes.Status201Created,
            ApiResponse.Ok(new
            {
                id = user.Id,
                email = user.Email,
                firstName = user.FirstName,
                lastName = user.LastName,
                role = user.Role
            }, "User created successfully"));
    }
}
