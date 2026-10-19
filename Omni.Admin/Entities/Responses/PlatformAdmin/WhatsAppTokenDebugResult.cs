namespace Omni.Admin.Entities.Responses.PlatformAdmin;

/// <summary>What Meta says about a token: granted permissions and the client WhatsApp business accounts it can see.
/// Copy of the main app's WhatsAppTokenDebugResult.</summary>
public record WhatsAppTokenDebugResult(IReadOnlyList<string> Scopes, IReadOnlyList<ClientWabaInfo> ClientWabas);

/// <summary>One client WhatsApp business account visible to the token. Copy of the main app's ClientWabaInfo.</summary>
public record ClientWabaInfo(string BusinessId, string BusinessName, string WabaId, string WabaName, string Relationship);