namespace Omni.Admin.Entities.Requests.Contacts;

public record ContactUpdate(string StageKey, string Qualification, bool MarketingOptIn, string? Notes);
