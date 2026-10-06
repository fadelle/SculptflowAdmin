using SculptFlowAdmin.Entities.Dtos.Channels;
using SculptFlowAdmin.Entities.Models;

namespace SculptFlowAdmin.Persistence.Contracts.Channels;

public interface IChannelAdminRepository
{
    Task<List<ChannelRow>> ListChannelsAsync(Guid? clinicId, string? channel, bool problemsOnly, CancellationToken ct = default);

    Task<ChannelIntegration?> GetChannelAsync(Guid id, CancellationToken ct = default);

    Task<List<WhatsAppHealthEvent>> HealthEventsAsync(Guid channelIntegrationId, CancellationToken ct = default);

    Task<List<CalendarRow>> ListCalendarsAsync(Guid? clinicId, CancellationToken ct = default);

    Task<List<TikTokRow>> ListTikTokAsync(Guid? clinicId, CancellationToken ct = default);

    /// <summary>Tracked.</summary>
    Task<ChannelIntegration?> GetChannelForUpdateAsync(Guid id, CancellationToken ct = default);

    Task<bool> ClinicExistsAsync(Guid clinicId, CancellationToken ct = default);

    /// <summary>Is this Infobip WhatsApp sender connected to a different clinic?</summary>
    Task<bool> IsInfobipSenderConnectedElsewhereAsync(string sender, Guid clinicId, CancellationToken ct = default);

    /// <summary>Tracked.</summary>
    Task<ChannelIntegration?> GetClinicChannelForUpdateAsync(Guid clinicId, string channel, CancellationToken ct = default);

    void AddChannel(ChannelIntegration integration);

    /// <summary>Tracked.</summary>
    Task<CalendarIntegration?> GetCalendarForUpdateAsync(Guid id, CancellationToken ct = default);

    /// <summary>Tracked.</summary>
    Task<TikTokIntegration?> GetTikTokForUpdateAsync(Guid id, CancellationToken ct = default);
}
