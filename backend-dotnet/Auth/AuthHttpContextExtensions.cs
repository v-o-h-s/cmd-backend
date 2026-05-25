namespace ClinicBackend.Api.Auth;

public static class AuthHttpContextExtensions
{
    public const string UserItemKey = "AuthenticatedUser";

    public static AuthenticatedUser? GetAuthenticatedUser(this HttpContext context)
    {
        return context.Items.TryGetValue(UserItemKey, out var value) ? value as AuthenticatedUser : null;
    }
}
