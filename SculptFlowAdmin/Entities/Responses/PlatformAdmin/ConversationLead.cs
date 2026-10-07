namespace SculptFlowAdmin.Entities.Responses.PlatformAdmin;

/// <summary>(Copy of the main app's Entities/Dtos/PlatformAdmin shape.) The lead a conversation belongs to, by name.</summary>
public record ConversationLead(Guid Id, string? FullName, string? FirstName, string? LastName, string? Phone);
