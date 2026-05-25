using ClinicBackend.Api.Auth;
using ClinicBackend.Api.Common;
using Microsoft.AspNetCore.Mvc;

namespace ClinicBackend.Api.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 2)
        {
            throw new ApiException("Invalid email or password", StatusCodes.Status400BadRequest, "ValidationError");
        }

        var (tokens, profile) = await _authService.LoginAsync(request.Email, request.Password);

        SetAuthCookies(tokens.AccessToken, tokens.RefreshToken, TimeSpan.FromDays(6));

        return Ok(ApiResponse.Ok(new
        {
            expiresIn = tokens.ExpiresIn,
            tokenType = tokens.TokenType,
            user = new
            {
                id = profile.Id,
                email = profile.Email,
                firstName = profile.FirstName,
                lastName = profile.LastName,
                role = profile.Role
            }
        }, "Login successful"));
    }

    [HttpPost("logout")]
    [RequireAuth]
    public IActionResult Logout()
    {
        ClearAuthCookies();
        return Ok(ApiResponse.Ok(null, "Logged out successfully"));
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new ApiException("Refresh token not found", StatusCodes.Status401Unauthorized, "AuthenticationError");
        }

        var tokens = await _authService.RefreshAsync(refreshToken);

        SetAuthCookies(tokens.AccessToken, tokens.RefreshToken, TimeSpan.FromHours(1));

        return Ok(ApiResponse.Ok(null, "Tokens refreshed successfully"));
    }

    [HttpPost("me")]
    [RequireAuth]
    public IActionResult Me()
    {
        var user = HttpContext.GetAuthenticatedUser();
        if (user is null)
        {
            throw new ApiException("User not authenticated", StatusCodes.Status401Unauthorized, "AuthenticationError");
        }

        return Ok(ApiResponse.Ok(new
        {
            id = user.Id,
            email = user.Email,
            firstName = user.FirstName,
            lastName = user.LastName,
            role = user.Role
        }, "User retrieved successfully"));
    }

    private void SetAuthCookies(string accessToken, string refreshToken, TimeSpan accessTokenTtl)
    {
        var secure = HttpContext.Request.IsHttps;

        Response.Cookies.Append("accessToken", accessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = SameSiteMode.Strict,
            MaxAge = accessTokenTtl,
            Path = "/"
        });

        Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = SameSiteMode.Strict,
            MaxAge = TimeSpan.FromDays(7),
            Path = "/auth/refresh-token"
        });
    }

    private void ClearAuthCookies()
    {
        var secure = HttpContext.Request.IsHttps;

        Response.Cookies.Delete("accessToken", new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = SameSiteMode.Strict,
            Path = "/"
        });

        Response.Cookies.Delete("refreshToken", new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = SameSiteMode.Strict,
            Path = "/auth/refresh-token"
        });
    }
}
