using SculptFlowAdmin.Entities.Dtos.Channels;
using SculptFlowAdmin.Entities.Responses.PlatformAdmin;

namespace SculptFlowAdmin.Business.Contracts.HttpClients.MainApp;

public interface IChannelsApiClient
{
    Task<List<ChannelRow>> ListAsync(Guid? clinicId, string? channel, bool problemsOnly, CancellationToken ct);

    Task<ChannelDetail?> GetAsync(Guid id, CancellationToken ct);

    Task<List<HealthEventRow>> HealthEventsAsync(Guid id, CancellationToken ct);

    Task<PlatformAdminChange> DisconnectAsync(Guid id, CancellationToken ct);

    Task<ChannelDetail> ConnectInfobipSenderAsync(Guid clinicId, string? sender, CancellationToken ct);

    Task<List<CalendarRow>> ListCalendarsAsync(Guid? clinicId, CancellationToken ct);

    Task<PlatformAdminChange> DisconnectCalendarAsync(Guid id, CancellationToken ct);

    Task<PlatformAdminChange> SetCalendarSyncAsync(Guid id, bool enabled, CancellationToken ct);

    Task<List<TikTokRow>> ListTikTokAsync(Guid? clinicId, CancellationToken ct);

    Task<PlatformAdminChange> DisconnectTikTokAsync(Guid id, CancellationToken ct);
}
