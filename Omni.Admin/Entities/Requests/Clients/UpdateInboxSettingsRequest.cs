namespace Omni.Admin.Entities.Requests.Clients;

/// <summary>(Copy of the main app's Entities/Requests/Inbox shape, PUT /api/platform-admin/clients/{id}/inbox-settings.) A
/// client's Inbox settings: whether the AI bot answers new conversations and conversations a contact reopens.</summary>
public record UpdateInboxSettingsRequest(bool AiBotNewConversations, bool AiBotReopenedConversations);
