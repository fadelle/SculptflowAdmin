using Omni.Admin.Entities.Dtos.Admins;

namespace Omni.Admin.Business.Contracts.HttpClients.MainApp;

public interface IAuthApiClient
{
    /// <summary>The admin, or null when the main app refuses the sign-in (wrong email or password, locked, inactive, not an
    /// admin: the main app answers one 401 for all of them). Throws MainAppApiException when the main app can't answer.</summary>
    Task<AdminIdentity?> LoginAsync(string email, string password, CancellationToken ct);

    /// <summary>The admin while still an active admin, else null (404). Throws MainAppApiException when the main app can't answer.</summary>
    Task<AdminIdentity?> GetActiveAdminAsync(string id, CancellationToken ct);
}
