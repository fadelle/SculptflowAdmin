using Microsoft.Extensions.Options;
using Omni.Admin.Business.Contracts.HttpClients.MainApp;
using Omni.Admin.Common.Configs;
using Omni.Admin.Entities.Dtos.Channels;
using Omni.Admin.Entities.Requests.PlatformAdmin;
using Omni.Admin.Entities.Responses.PlatformAdmin;

namespace Omni.Admin.Business.HttpClients.MainApp;

/// <summary>Client for the main app's platform-admin channels API (/api/platform-admin/channels): messaging, calendar
/// and TikTok connections.</summary>
public class ChannelsApiClient : MainAppApiClient, IChannelsApiClient
{
    public const string Prefix = "api/platform-admin/channels";

    public ChannelsApiClient(HttpClient http, IOptions<MainAppApiOptions> options, IHttpContextAccessor context, ILogger<ChannelsApiClient> logger)
        : base(http, options, context, logger, Prefix, "Channels")
    {
    }

    public Task<List<ChannelRow>> ListAsync(Guid? clientId, string? channel, bool problemsOnly, CancellationToken ct) =>
        GetRequiredAsync<List<ChannelRow>>(Query(("clientId", clientId), ("channel", channel), ("problemsOnly", problemsOnly)), ct);

    public Task<ChannelDetail?> GetAsync(Guid id, CancellationToken ct) => GetAsync<ChannelDetail>($"/{id}", ct);

    public Task<List<HealthEventRow>> HealthEventsAsync(Guid id, CancellationToken ct) =>
        GetRequiredAsync<List<HealthEventRow>>($"/{id}/health-events", ct);

    public Task<PlatformAdminChange> DisconnectAsync(Guid id, CancellationToken ct) =>
        WriteForAsync<PlatformAdminChange>(HttpMethod.Post, $"/{id}/disconnect", null, null, false, ct);

    public Task<List<CalendarRow>> ListCalendarsAsync(Guid? clientId, CancellationToken ct) =>
        GetRequiredAsync<List<CalendarRow>>("/calendars" + Query(("clientId", clientId)), ct);

    public Task<PlatformAdminChange> DisconnectCalendarAsync(Guid id, CancellationToken ct) =>
        WriteForAsync<PlatformAdminChange>(HttpMethod.Post, $"/calendars/{id}/disconnect", null, null, false, ct);

    public Task<PlatformAdminChange> SetCalendarSyncAsync(Guid id, bool enabled, CancellationToken ct) =>
        WriteForAsync<PlatformAdminChange>(HttpMethod.Put, $"/calendars/{id}/sync", new FlagBody(enabled), null, false, ct);

    public Task<List<TikTokRow>> ListTikTokAsync(Guid? clientId, CancellationToken ct) =>
        GetRequiredAsync<List<TikTokRow>>("/tiktok" + Query(("clientId", clientId)), ct);

    public Task<WhatsAppTokenDebugResult> DebugTokenAsync(DebugTokenRequest request, CancellationToken ct) =>
        WriteForAsync<WhatsAppTokenDebugResult>(HttpMethod.Post, "/whatsapp/debug-token", request, null, false, ct);

    public Task<PlatformAdminChange> DisconnectTikTokAsync(Guid id, CancellationToken ct) =>
        WriteForAsync<PlatformAdminChange>(HttpMethod.Post, $"/tiktok/{id}/disconnect", null, null, false, ct);
}
