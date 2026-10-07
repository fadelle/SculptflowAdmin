using Microsoft.Extensions.Options;
using SculptFlowAdmin.Business.Contracts.HttpClients.MainApp;
using SculptFlowAdmin.Common.Configs;
using SculptFlowAdmin.Entities.Dtos.Channels;
using SculptFlowAdmin.Entities.Requests.Channels;
using SculptFlowAdmin.Entities.Requests.PlatformAdmin;
using SculptFlowAdmin.Entities.Responses.PlatformAdmin;

namespace SculptFlowAdmin.Business.HttpClients.MainApp;

/// <summary>Client for the main app's platform-admin channels API (/api/platform-admin/channels): messaging, calendar
/// and TikTok connections.</summary>
public class ChannelsApiClient : MainAppApiClient, IChannelsApiClient
{
    public const string Prefix = "api/platform-admin/channels";

    public ChannelsApiClient(HttpClient http, IOptions<MainAppApiOptions> options, IHttpContextAccessor context, ILogger<ChannelsApiClient> logger)
        : base(http, options, context, logger, Prefix, "Channels")
    {
    }

    public Task<List<ChannelRow>> ListAsync(Guid? clinicId, string? channel, bool problemsOnly, CancellationToken ct) =>
        GetRequiredAsync<List<ChannelRow>>(Query(("clinicId", clinicId), ("channel", channel), ("problemsOnly", problemsOnly)), ct);

    public Task<ChannelDetail?> GetAsync(Guid id, CancellationToken ct) => GetAsync<ChannelDetail>($"/{id}", ct);

    public Task<List<HealthEventRow>> HealthEventsAsync(Guid id, CancellationToken ct) =>
        GetRequiredAsync<List<HealthEventRow>>($"/{id}/health-events", ct);

    public Task<PlatformAdminChange> DisconnectAsync(Guid id, CancellationToken ct) =>
        WriteForAsync<PlatformAdminChange>(HttpMethod.Post, $"/{id}/disconnect", null, null, false, ct);

    public Task<ChannelDetail> ConnectInfobipSenderAsync(Guid clinicId, string? sender, CancellationToken ct) =>
        WriteForAsync<ChannelDetail>(HttpMethod.Post, $"/whatsapp/infobip/{clinicId}", new InfobipSenderBody(sender), null, false, ct);

    public Task<List<CalendarRow>> ListCalendarsAsync(Guid? clinicId, CancellationToken ct) =>
        GetRequiredAsync<List<CalendarRow>>("/calendars" + Query(("clinicId", clinicId)), ct);

    public Task<PlatformAdminChange> DisconnectCalendarAsync(Guid id, CancellationToken ct) =>
        WriteForAsync<PlatformAdminChange>(HttpMethod.Post, $"/calendars/{id}/disconnect", null, null, false, ct);

    public Task<PlatformAdminChange> SetCalendarSyncAsync(Guid id, bool enabled, CancellationToken ct) =>
        WriteForAsync<PlatformAdminChange>(HttpMethod.Put, $"/calendars/{id}/sync", new FlagBody(enabled), null, false, ct);

    public Task<List<TikTokRow>> ListTikTokAsync(Guid? clinicId, CancellationToken ct) =>
        GetRequiredAsync<List<TikTokRow>>("/tiktok" + Query(("clinicId", clinicId)), ct);

    public Task<PlatformAdminChange> DisconnectTikTokAsync(Guid id, CancellationToken ct) =>
        WriteForAsync<PlatformAdminChange>(HttpMethod.Post, $"/tiktok/{id}/disconnect", null, null, false, ct);
}
