namespace Omni.Admin.Entities.Requests.Leads;

public record LeadUpdate(string Status, string QualificationStatus, bool MarketingOptIn, string? Notes);
