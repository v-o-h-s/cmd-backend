using ClinicBackend.Api.Common;
using ClinicBackend.Api.Users;

namespace ClinicBackend.Api.Auth;

public sealed class AuthService
{
    private readonly SupabaseAuthClient _authClient;
    private readonly UserRepository _userRepository;

    public AuthService(SupabaseAuthClient authClient, UserRepository userRepository)
    {
        _authClient = authClient;
        _userRepository = userRepository;
    }

    public async Task<(AuthTokenResponse Tokens, ProfileRecord Profile)> LoginAsync(string email, string password)
    {
        var auth = await _authClient.LoginAsync(email, password);

        if (auth.User is null || string.IsNullOrWhiteSpace(auth.User.Id))
        {
            throw new ApiException("User not found", StatusCodes.Status400BadRequest, "ValidationError");
        }

        var profile = await _userRepository.FindByAuthUuidAsync(auth.User.Id);
        if (profile is null)
        {
            throw new ApiException("User profile not found", StatusCodes.Status404NotFound, "DatabaseError");
        }

        return (auth, profile);
    }

    public Task<AuthTokenResponse> RefreshAsync(string refreshToken)
    {
        return _authClient.RefreshAsync(refreshToken);
    }

    public async Task<ProfileRecord> GetProfileFromTokenAsync(string accessToken)
    {
        var userDoc = await _authClient.GetUserFromAccessTokenAsync(accessToken);
        var id = userDoc.RootElement.GetProperty("id").GetString();
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ApiException("Invalid or expired token", StatusCodes.Status401Unauthorized, "AuthenticationError");
        }

        var profile = await _userRepository.FindByAuthUuidAsync(id);
        if (profile is null)
        {
            throw new ApiException("User profile not found", StatusCodes.Status401Unauthorized, "AuthenticationError");
        }

        return profile;
    }
}
