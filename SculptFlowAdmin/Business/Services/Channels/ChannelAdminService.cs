using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;
using SculptFlowAdmin.Business.Contracts.Managers;
using SculptFlowAdmin.Business.Contracts.Services.Channels;
using SculptFlowAdmin.Common.Enums;
using SculptFlowAdmin.Common.Exceptions;
using SculptFlowAdmin.Entities.Dtos.Channels;
using SculptFlowAdmin.Entities.Models;
using SculptFlowAdmin.Persistence.Contracts;
using SculptFlowAdmin.Persistence.Contracts.Channels;

namespace SculptFlowAdmin.Business.Services.Channels;

/// <summary>
/// Messaging channels (channel_integrations), calendar connections and TikTok connections across clinics.
/// Writes mirror the main app's own rules (ChannelIntegrationService.DisconnectAsync,
/// InfobipWhatsAppIntegrationService.ConnectAsync) so a row the admin touches looks exactly like one the clinic set up.
/// </summary>
public class ChannelAdminService : IChannelAdminService
{
    private readonly IChannelAdminRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAdminAudit _audit;
    private readonly IConfiguration _config;

    public ChannelAdminService(IChannelAdminRepository repository, IUnitOfWork unitOfWork, IAdminAudit audit, IConfiguration config)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _audit = audit;
        _config = config;
    }

    public Task<List<ChannelRow>> ListChannelsAsync(Guid? clinicId, string? channel, bool problemsOnly, CancellationToken ct = default) =>
        _repository.ListChannelsAsync(clinicId, channel, problemsOnly, ct);

    public Task<ChannelIntegration?> GetChannelAsync(Guid id, CancellationToken ct = default) =>
        _repository.GetChannelAsync(id, ct);

    public Task<List<WhatsAppHealthEvent>> HealthEventsAsync(Guid channelIntegrationId, CancellationToken ct = default) =>
        _repository.HealthEventsAsync(channelIntegrationId, ct);

    public Task<List<CalendarRow>> ListCalendarsAsync(Guid? clinicId, CancellationToken ct = default) =>
        _repository.ListCalendarsAsync(clinicId, ct);

    public Task<List<TikTokRow>> ListTikTokAsync(Guid? clinicId, CancellationToken ct = default) =>
        _repository.ListTikTokAsync(clinicId, ct);

    /// <summary>
    /// Same as a clinic clicking Disconnect. For Telegram the main app also deletes the webhook at Telegram; from here
    /// only the row is cleared, so Telegram keeps calling until the webhook fails the (now cleared) secret check.
    /// </summary>
    public async Task DisconnectAsync(Guid id, CancellationToken ct = default)
    {
        var row = await _repository.GetChannelForUpdateAsync(id, ct) ?? throw new KeyNotFoundException();
        row.Status = ChannelIntegrationStatus.Disconnected;
        row.AccessToken = null;
        row.WebhookVerifyToken = null;
        row.Pin = null;
        row.LastVerifiedAt = null;
        row.LastError = null;
        if (row.Channel == ChannelType.Telegram) row.WebhookStatus = WebhookStatus.NotRegistered;
        row.UpdatedAt = DateTimeOffset.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);
        await _audit.LogAsync("channel.disconnected", "channel_integration", id, row.ClinicId, new { row.Channel }, ct);
    }

    /// <summary>
    /// Connects (or moves) a clinic's WhatsApp to an Infobip sender number — the admin side of the main app's
    /// InfobipWhatsAppIntegrationService.ConnectAsync, minus the live check against Infobip's business-info API (the
    /// admin portal holds no Infobip key). Register the number as a sender in the Infobip portal first.
    /// </summary>
    public async Task<ChannelIntegration> ConnectInfobipSenderAsync(Guid clinicId, string? senderNumber, string? verifiedName,
        CancellationToken ct = default)
    {
        var sender = new string((senderNumber ?? "").Where(char.IsDigit).ToArray());
        if (sender.Length is < 8 or > 15) throw new AdminRuleException("Enter the number in international format, e.g. +44 7860 099299.");
        if (!await _repository.ClinicExistsAsync(clinicId, ct)) throw new KeyNotFoundException();

        var usedElsewhere = await _repository.IsInfobipSenderConnectedElsewhereAsync(sender, clinicId, ct);
        if (usedElsewhere) throw new AdminRuleException($"+{sender} is already connected to another clinic. Disconnect it there first.");

        var row = await _repository.GetClinicChannelForUpdateAsync(clinicId, ChannelType.WhatsApp, ct);
        var now = DateTimeOffset.UtcNow;
        if (row is null)
        {
            row = new ChannelIntegration { Id = Guid.NewGuid(), ClinicId = clinicId, Channel = ChannelType.WhatsApp, CreatedAt = now };
            _repository.AddChannel(row);
        }

        var sameConnection = ChannelProvider.Of(row) == ChannelProvider.Infobip && row.ProviderSenderId == sender
                             && !string.IsNullOrEmpty(row.WebhookVerifyToken);
        if (!sameConnection) row.WebhookVerifyToken = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));

        row.Provider = ChannelProvider.Infobip;
        row.ProviderSenderId = sender;
        row.DisplayName = "+" + sender;
        if (!string.IsNullOrWhiteSpace(verifiedName)) row.VerifiedName = verifiedName.Trim();
        row.PhoneNumberId = null;
        row.WhatsAppBusinessId = null;
        row.MetaBusinessId = null;
        row.AccessToken = null;
        row.Pin = null;
        row.Status = ChannelIntegrationStatus.Connected;
        row.LastVerifiedAt = now;
        row.LastError = null;
        row.IsHealthy = true;
        row.HealthLevel = WhatsAppHealthLevel.Healthy;
        row.UpdatedAt = now;
        await _unitOfWork.SaveChangesAsync(ct);
        await _audit.LogAsync("channel.infobip_connected", "channel_integration", row.Id, clinicId, new { sender }, ct);
        return row;
    }

    /// <summary>The inbound/status URL to paste into the Infobip sender (main app's InfobipWebhookUrls.Build).
    /// Null when MainApp:PublicBaseUrl isn't configured here or the row isn't a connected Infobip sender.</summary>
    public string? InfobipWebhookUrl(ChannelIntegration row)
    {
        var baseUrl = _config["MainApp:PublicBaseUrl"]?.Trim().TrimEnd('/');
        if (string.IsNullOrEmpty(baseUrl) || ChannelProvider.Of(row) != ChannelProvider.Infobip
            || row.Status != ChannelIntegrationStatus.Connected || string.IsNullOrEmpty(row.WebhookVerifyToken)) return null;
        return $"{baseUrl}/api/integrations/whatsapp/connections/{row.Id}/events?token={Uri.EscapeDataString(row.WebhookVerifyToken)}";
    }

    public string ActiveWhatsAppProvider => _config["MainApp:WhatsAppProvider"] ?? "unknown (set MainApp:WhatsAppProvider)";

    // ------------------------------------------------------------------ calendars / TikTok

    /// <summary>Same as the clinic's Disconnect, except the tokens are only forgotten, not revoked at Google/Microsoft
    /// (the admin portal holds no OAuth client secrets). The clinic reconnects from Settings → Calendar Integrations.</summary>
    public async Task DisconnectCalendarAsync(Guid id, CancellationToken ct = default)
    {
        var c = await _repository.GetCalendarForUpdateAsync(id, ct) ?? throw new KeyNotFoundException();
        c.Status = CalendarIntegrationStatus.Disconnected;
        c.ExternalConnectionRef = null;
        c.AccountDisplayName = null;
        c.SelectedCalendarId = null;
        c.SelectedCalendarName = null;
        c.SyncEnabled = false;
        c.IsHealthy = true;
        c.LastProblemMessage = null;
        c.AccessToken = null;
        c.RefreshToken = null;
        c.TokenExpiresAt = null;
        c.UpdatedAt = DateTimeOffset.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);
        await _audit.LogAsync("calendar.disconnected", "calendar_integration", id, c.ClinicId, new { c.Provider }, ct);
    }

    public async Task SetCalendarSyncAsync(Guid id, bool enabled, CancellationToken ct = default)
    {
        var c = await _repository.GetCalendarForUpdateAsync(id, ct) ?? throw new KeyNotFoundException();
        if (enabled && c.Status != CalendarIntegrationStatus.Connected) throw new AdminRuleException("Only a connected calendar can sync.");
        c.SyncEnabled = enabled;
        c.UpdatedAt = DateTimeOffset.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);
        await _audit.LogAsync(enabled ? "calendar.sync_enabled" : "calendar.sync_disabled", "calendar_integration", id, c.ClinicId, ct: ct);
    }

    public async Task DisconnectTikTokAsync(Guid id, CancellationToken ct = default)
    {
        var c = await _repository.GetTikTokForUpdateAsync(id, ct) ?? throw new KeyNotFoundException();
        c.Status = TikTokIntegrationStatus.Disconnected;
        c.AccessToken = null;
        c.RefreshToken = null;
        c.TokenExpiresAt = null;
        c.RefreshTokenExpiresAt = null;
        c.UpdatedAt = DateTimeOffset.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);
        await _audit.LogAsync("tiktok.disconnected", "tiktok_integration", id, c.ClinicId, ct: ct);
    }
}
