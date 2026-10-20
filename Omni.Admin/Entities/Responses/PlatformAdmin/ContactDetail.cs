namespace Omni.Admin.Entities.Responses.PlatformAdmin;

/// <summary>(Copy of the main app's Entities/Dtos/PlatformAdmin shape.) One contact for the admin portal.</summary>
public record ContactDetail(Guid Id, Guid ClientId, NamedRef? Client, Guid? ServiceId, NamedRef? Service, string? FullName,
    string? FirstName, string? LastName, string? Phone, string? Email, string? Source, string? SourceDetail, string? CampaignName,
    string? ExternalContactId, string StageKey, string Qualification, string? PreferredLanguage, string? CountryCode, string? City,
    string? DesiredTimeline, string? Notes, bool MarketingOptIn, DateTimeOffset? OptedOutAt, DateTimeOffset? LastContactAt,
    DateTimeOffset? NextFollowupAt, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);
