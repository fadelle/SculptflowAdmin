using Omni.Admin.Entities.Dtos.Admins;

namespace Omni.Admin.Business.Contracts.HttpClients.MainApp;

public interface IAdminsApiClient
{
    Task<IReadOnlyList<AdminRow>> ListAsync(CancellationToken ct);

    Task<AdminRow> CreateAsync(string? email, string? fullName, string? password, CancellationToken ct);

    Task<AdminRow> SetActiveAsync(string id, bool active, string? actingAdminId, CancellationToken ct);

    Task<AdminRow> SetPasswordAsync(string id, string? password, CancellationToken ct);
}
