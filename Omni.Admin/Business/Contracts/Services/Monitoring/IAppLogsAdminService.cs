using Omni.Admin.Entities.Responses.Monitoring;

namespace Omni.Admin.Business.Contracts.Services.Monitoring;

/// <summary>The main app's saved warnings and errors, through its platform-admin API. Read-only, so nothing is audited.</summary>
public interface IAppLogsAdminService
{
    /// <summary>One row per problem; <paramref name="level"/> is a minimum (warning, error, critical), sort recent or count.</summary>
    Task<AppLogGroupReport> GroupsAsync(int hours, string level, Guid? clinicId, string? search, string sort, CancellationToken ct);

    /// <summary>Single rows newest first, optionally one problem (fingerprint) or one trace.</summary>
    Task<AppLogPage> ListAsync(int hours, string level, Guid? clinicId, string? search, string? fingerprint, string? traceId, int page,
        CancellationToken ct);

    /// <summary>One row in full; null when it doesn't exist (or was purged).</summary>
    Task<AppLogDetail?> GetAsync(long id, CancellationToken ct);

    /// <summary>The dashboard tile's counts, or null when the main app can't answer (older main app, unreachable): the
    /// dashboard must still load.</summary>
    Task<AppLogSummary?> TrySummaryAsync(CancellationToken ct);
}