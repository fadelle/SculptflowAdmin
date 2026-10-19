namespace Omni.Admin.Entities.Requests.PlatformAdmin;

/// <summary>Token to inspect: an access token, or an embedded-signup code to exchange first. Copy of the main app's
/// DebugTokenRequest (<c>POST /api/platform-admin/channels/whatsapp/debug-token</c>).</summary>
public record DebugTokenRequest(string? Code = null, string? AccessToken = null, string? RedirectUri = null);