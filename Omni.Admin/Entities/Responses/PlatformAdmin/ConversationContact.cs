namespace Omni.Admin.Entities.Responses.PlatformAdmin;

/// <summary>(Copy of the main app's Entities/Dtos/PlatformAdmin shape.) The contact a conversation belongs to, by name.</summary>
public record ConversationContact(Guid Id, string? FullName, string? FirstName, string? LastName, string? Phone);
