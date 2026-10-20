namespace Omni.Admin.Entities.Dtos.Contacts;

public record ContactRow(Guid Id, Guid ClientId, string ClientName, string Name, string? Phone, string? Email, string? Source,
    string StageKey, string Qualification, string? ServiceName, bool MarketingOptIn, DateTimeOffset? LastContactAt,
    DateTimeOffset CreatedAt);
