using Omni.Admin.Business.Contracts.HttpClients.MainApp;
using Omni.Admin.Business.Contracts.Managers;
using Omni.Admin.Business.Contracts.Services.Channels;
using Omni.Admin.Entities.Dtos.Channels;
using Omni.Admin.Entities.Requests.PlatformAdmin;
using Omni.Admin.Entities.Responses.PlatformAdmin;

namespace Omni.Admin.Business.Services.Channels;

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

    public async Task<WhatsAppTokenDebugResult> DebugWhatsAppTokenAsync(string? accessToken, string? code, string? redirectUri, CancellationToken ct = default)
    {
        accessToken = string.IsNullOrWhiteSpace(accessToken) ? null : accessToken.Trim();
        code = string.IsNullOrWhiteSpace(code) ? null : code.Trim();
        if (accessToken is null && code is null)
        {
            throw new ArgumentException("Paste an access token, or an embedded-signup code.");
        }

        var result = await _api.DebugTokenAsync(
            new DebugTokenRequest(code, accessToken, string.IsNullOrWhiteSpace(redirectUri) ? null : redirectUri.Trim()), ct);
        // The token itself is never written anywhere: the audit entry records only that an inspection happened.
        await _audit.LogAsync("whatsapp.token_inspected", "whatsapp_token", null, null,
            new { input = accessToken is not null ? "access_token" : "code", scopes = result.Scopes.Count, wabas = result.ClientWabas.Count }, ct);
        return result;
    }

    public async Task DisconnectTikTokAsync(Guid id, CancellationToken ct = default)
    {
        var change = await _api.DisconnectTikTokAsync(id, ct);
        await _audit.LogAsync("tiktok.disconnected", "tiktok_integration", id, change.ClinicId, ct: ct);
    }
}
