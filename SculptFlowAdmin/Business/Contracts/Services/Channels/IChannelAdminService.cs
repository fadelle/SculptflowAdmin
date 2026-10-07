using SculptFlowAdmin.Entities.Dtos.Channels;
using SculptFlowAdmin.Entities.Responses.PlatformAdmin;

namespace SculptFlowAdmin.Business.Contracts.Services.Channels;

/// <summary>Messaging, calendar and TikTok connections, through the main app's channels API; every change is audited.</summary>
public interface IChannelAdminService
{
    Task<List<ChannelRow>> ListChannelsAsync(Guid? clinicId, string? channel, bool problemsOnly, CancellationToken ct = default);

    Task<ChannelDetail?> GetChannelAsync(Guid id, CancellationToken ct = default);

    Task<List<HealthEventRow>> HealthEventsAsync(Guid channelIntegrationId, CancellationToken ct = default);

    Task<List<CalendarRow>> ListCalendarsAsync(Guid? clinicId, CancellationToken ct = default);

    Task<List<TikTokRow>> ListTikTokAsync(Guid? clinicId, CancellationToken ct = default);

    Task DisconnectAsync(Guid id, CancellationToken ct = default);

    /// <summary>The main app checks the number with Infobip and connects (or moves) the clinic's WhatsApp to it.</summary>
    Task<ChannelDetail> ConnectInfobipSenderAsync(Guid clinicId, string? senderNumber, CancellationToken ct = default);

    Task DisconnectCalendarAsync(Guid id, CancellationToken ct = default);

    Task SetCalendarSyncAsync(Guid id, bool enabled, CancellationToken ct = default);

    Task DisconnectTikTokAsync(Guid id, CancellationToken ct = default);

    string ActiveWhatsAppProvider { get; }
}
