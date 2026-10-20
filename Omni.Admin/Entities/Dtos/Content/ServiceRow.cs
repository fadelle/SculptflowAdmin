namespace Omni.Admin.Entities.Dtos.Content;

public record ServiceRow(Guid Id, Guid ClientId, string ClientName, string Name, string? Code, int? BookingDuration,
    bool IsActive, int Contacts, DateTimeOffset UpdatedAt);
