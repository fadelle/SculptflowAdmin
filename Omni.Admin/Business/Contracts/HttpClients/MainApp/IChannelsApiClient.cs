using Omni.Admin.Entities.Dtos.Channels;
using Omni.Admin.Entities.Requests.PlatformAdmin;
using Omni.Admin.Entities.Responses.PlatformAdmin;

namespace Omni.Admin.Business.Contracts.HttpClients.MainApp;

public interface IChannelsApiClient
{
    Task<List<ChannelRow>> ListAsync(Guid? clientId, string? channel, bool problemsOnly, CancellationToken ct);

    Task<ChannelDetail?> GetAsync(Guid id, CancellationToken ct);

    Task<List<HealthEventRow>> HealthEventsAsync(Guid id, CancellationToken ct);

    Task<PlatformAdminChange> DisconnectAsync(Guid id, CancellationToken ct);

    Task<List<CalendarRow>> ListCalendarsAsync(Guid? clientId, CancellationToken ct);

    Task<PlatformAdminChange> DisconnectCalendarAsync(Guid id, CancellationToken ct);

    Task<PlatformAdminChange> SetCalendarSyncAsync(Guid id, bool enabled, CancellationToken ct);

    Task<List<TikTokRow>> ListTikTokAsync(Guid? clientId, CancellationToken ct);

    Task<PlatformAdminChange> DisconnectTikTokAsync(Guid id, CancellationToken ct);

    /// <summary>Inspect an access token (or exchange an embedded-signup code first) with the platform's own Meta app.</summary>
    Task<WhatsAppTokenDebugResult> DebugTokenAsync(DebugTokenRequest request, CancellationToken ct);
}
