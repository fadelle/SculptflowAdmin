namespace Omni.Admin.Entities.Requests.Clinics;

/// <summary>(Copy of the main app's Entities/Requests/Inbox shape, PUT /api/platform-admin/clinics/{id}/inbox-settings.) A
/// clinic's Inbox settings: whether the AI bot answers new conversations and conversations a patient reopens.</summary>
public record UpdateInboxSettingsRequest(bool AiBotNewConversations, bool AiBotReopenedConversations);
