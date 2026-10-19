using Omni.Admin.Entities.Dtos.Channels;
using Omni.Admin.Entities.Responses.PlatformAdmin;

namespace Omni.Admin.Business.Contracts.Services.Channels;

/// <summary>Messaging, calendar and TikTok connections, through the main app's channels API; every change is audited.</summary>
public interface IChannelAdminService
{
    Task<List<ChannelRow>> ListChannelsAsync(Guid? clinicId, string? channel, bool problemsOnly, CancellationToken ct = default);

    Task<ChannelDetail?> GetChannelAsync(Guid id, CancellationToken ct = default);

    Task<List<HealthEventRow>> HealthEventsAsync(Guid channelIntegrationId, CancellationToken ct = default);

    Task<List<CalendarRow>> ListCalendarsAsync(Guid? clinicId, CancellationToken ct = default);

    Task<List<TikTokRow>> ListTikTokAsync(Guid? clinicId, CancellationToken ct = default);

    Task DisconnectAsync(Guid id, CancellationToken ct = default);

    Task DisconnectCalendarAsync(Guid id, CancellationToken ct = default);

    Task SetCalendarSyncAsync(Guid id, bool enabled, CancellationToken ct = default);

    Task DisconnectTikTokAsync(Guid id, CancellationToken ct = default);

    /// <summary>Inspects an access token or embedded-signup code. Neither is stored or audited; only that a check was made.</summary>
    Task<WhatsAppTokenDebugResult> DebugWhatsAppTokenAsync(string? accessToken, string? code, string? redirectUri, CancellationToken ct = default);
}
