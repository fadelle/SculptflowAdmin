using SculptFlowAdmin.Business.Contracts.HttpClients.MainApp;
using SculptFlowAdmin.Business.Contracts.Managers;
using SculptFlowAdmin.Business.Contracts.Services.Channels;
using SculptFlowAdmin.Entities.Dtos.Channels;
using SculptFlowAdmin.Entities.Responses.PlatformAdmin;

namespace SculptFlowAdmin.Business.Services.Channels;

/// <summary>
/// The main app does every change with the same services a clinic uses (Telegram webhook removal, calendar token
/// revocation, plan limits); this service audits each one.
/// </summary>
public class ChannelAdminService : IChannelAdminService
{
    private readonly IChannelsApiClient _api;
    private readonly IAdminAudit _audit;

    public ChannelAdminService(IChannelsApiClient api, IAdminAudit audit)
    {
        _api = api;
        _audit = audit;
    }

    public Task<List<ChannelRow>> ListChannelsAsync(Guid? clinicId, string? channel, bool problemsOnly, CancellationToken ct = default) =>
        _api.ListAsync(clinicId, channel, problemsOnly, ct);

    public Task<ChannelDetail?> GetChannelAsync(Guid id, CancellationToken ct = default) => _api.GetAsync(id, ct);

    public Task<List<HealthEventRow>> HealthEventsAsync(Guid channelIntegrationId, CancellationToken ct = default) =>
        _api.HealthEventsAsync(channelIntegrationId, ct);

    public Task<List<CalendarRow>> ListCalendarsAsync(Guid? clinicId, CancellationToken ct = default) => _api.ListCalendarsAsync(clinicId, ct);

    public Task<List<TikTokRow>> ListTikTokAsync(Guid? clinicId, CancellationToken ct = default) => _api.ListTikTokAsync(clinicId, ct);

    public async Task DisconnectAsync(Guid id, CancellationToken ct = default)
    {
        var change = await _api.DisconnectAsync(id, ct);
        await _audit.LogAsync("channel.disconnected", "channel_integration", id, change.ClinicId, ct: ct);
    }

    public async Task DisconnectCalendarAsync(Guid id, CancellationToken ct = default)
    {
        var change = await _api.DisconnectCalendarAsync(id, ct);
        await _audit.LogAsync("calendar.disconnected", "calendar_integration", id, change.ClinicId, ct: ct);
    }

    public async Task SetCalendarSyncAsync(Guid id, bool enabled, CancellationToken ct = default)
    {
        var change = await _api.SetCalendarSyncAsync(id, enabled, ct);
        await _audit.LogAsync(enabled ? "calendar.sync_enabled" : "calendar.sync_disabled", "calendar_integration", id, change.ClinicId, ct: ct);
    }

    public async Task DisconnectTikTokAsync(Guid id, CancellationToken ct = default)
    {
        var change = await _api.DisconnectTikTokAsync(id, ct);
        await _audit.LogAsync("tiktok.disconnected", "tiktok_integration", id, change.ClinicId, ct: ct);
    }
}
