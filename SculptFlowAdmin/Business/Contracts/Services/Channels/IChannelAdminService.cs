using SculptFlowAdmin.Entities.Dtos.Channels;
using SculptFlowAdmin.Entities.Models;

namespace SculptFlowAdmin.Business.Contracts.Services.Channels;

public interface IChannelAdminService
{
    Task<List<ChannelRow>> ListChannelsAsync(Guid? clinicId, string? channel, bool problemsOnly, CancellationToken ct = default);

    Task<ChannelIntegration?> GetChannelAsync(Guid id, CancellationToken ct = default);

    Task<List<WhatsAppHealthEvent>> HealthEventsAsync(Guid channelIntegrationId, CancellationToken ct = default);

    Task<List<CalendarRow>> ListCalendarsAsync(Guid? clinicId, CancellationToken ct = default);

    Task<List<TikTokRow>> ListTikTokAsync(Guid? clinicId, CancellationToken ct = default);

    /// <summary>
    /// Same as a clinic clicking Disconnect. For Telegram the main app also deletes the webhook at Telegram; from here
    /// only the row is cleared, so Telegram keeps calling until the webhook fails the (now cleared) secret check.
    /// </summary>
    Task DisconnectAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Connects (or moves) a clinic's WhatsApp to an Infobip sender number — the admin side of the main app's
    /// InfobipWhatsAppIntegrationService.ConnectAsync, minus the live check against Infobip's business-info API (the
    /// admin portal holds no Infobip key). Register the number as a sender in the Infobip portal first.
    /// </summary>
    Task<ChannelIntegration> ConnectInfobipSenderAsync(Guid clinicId, string? senderNumber, string? verifiedName,
        CancellationToken ct = default);

    /// <summary>The inbound/status URL to paste into the Infobip sender (main app's InfobipWebhookUrls.Build).
    /// Null when MainApp:PublicBaseUrl isn't configured here or the row isn't a connected Infobip sender.</summary>
    string? InfobipWebhookUrl(ChannelIntegration row);

    /// <summary>Same as the clinic's Disconnect, except the tokens are only forgotten, not revoked at Google/Microsoft
    /// (the admin portal holds no OAuth client secrets). The clinic reconnects from Settings → Calendar Integrations.</summary>
    Task DisconnectCalendarAsync(Guid id, CancellationToken ct = default);

    Task SetCalendarSyncAsync(Guid id, bool enabled, CancellationToken ct = default);

    Task DisconnectTikTokAsync(Guid id, CancellationToken ct = default);

    string ActiveWhatsAppProvider { get; }
}
