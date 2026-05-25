using System.Text.Json;
using ClinicBackend.Api.Common;
using ClinicBackend.Api.Infrastructure;

namespace ClinicBackend.Api.Users;

public sealed class UserRepository
{
    private readonly SupabaseRestClient _restClient;

    public UserRepository(SupabaseRestClient restClient)
    {
        _restClient = restClient;
    }

    public async Task<ProfileRecord?> FindByAuthUuidAsync(string authUuid)
    {
        try
        {
            var doc = await _restClient.SelectSingleAsync(
                "profiles",
                $"id=eq.{Uri.EscapeDataString(authUuid)}&select=id,email,first_name,last_name,role"
            );

            return JsonSerializer.Deserialize<ProfileRecord>(doc.RootElement.GetRawText(), JsonOptions.CamelCase());
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    }

    public async Task<Dictionary<string, int>> CountStaffAsync()
    {
        var doctors = await _restClient.SelectManyAsync("profiles", "select=id&role=eq.doctor");
        var receptionists = await _restClient.SelectManyAsync("profiles", "select=id&role=eq.receptionist");

        var doctorsCount = doctors.RootElement.GetArrayLength();
        var receptionistsCount = receptionists.RootElement.GetArrayLength();

        return new Dictionary<string, int>
        {
            ["doctors"] = doctorsCount,
            ["receptionists"] = receptionistsCount,
            ["total"] = doctorsCount + receptionistsCount
        };
    }
}
