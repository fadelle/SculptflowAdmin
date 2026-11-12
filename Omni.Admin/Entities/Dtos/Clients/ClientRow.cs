namespace Omni.Admin.Entities.Dtos.Clients;

public record ClientRow(Guid Id, string Name, string Slug, string? Email, string Timezone, bool IsActive,
    int MemberCount, int ContactCount, int ConversationCount, DateTimeOffset? LastMessageAt, string WhatsAppStatus,
    DateTimeOffset CreatedAt)
{
    /// <summary>Each channel with its status (QA t6-admin-24); empty from a main app older than that.</summary>
    public IReadOnlyList<ClientChannelState> Channels { get; init; } = [];
}

public record ClientChannelState(string Channel, string Status);
