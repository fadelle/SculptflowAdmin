namespace Omni.Admin.Entities.Responses.PlatformAdmin;

/// <summary>(Copy of the main app's Entities/Dtos/PlatformAdmin shape.) One client for the admin portal.</summary>
public record ClientDetail(Guid Id, string Name, string Slug, string? Phone, string? Email, string? Website, string? CountryCode,
    string Timezone, string? Address, string? OperatingHours, string? BookingInfo, bool IsActive, DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string BusinessTypeCode, string DefaultLanguage);
