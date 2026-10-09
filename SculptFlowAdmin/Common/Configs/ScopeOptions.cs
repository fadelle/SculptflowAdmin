namespace SculptFlowAdmin.Common.Configs;

/// <summary>Names of the global scope (config section "Scope", optional), so "Clinic" → something else later is
/// configuration, not a rewrite. QueryKey is the pages' existing clinic filter parameter.</summary>
public sealed class ScopeOptions
{
    public string EntityLabel { get; set; } = "Clinic";
    public string EntityLabelPlural { get; set; } = "Clinics";
    public string QueryKey { get; set; } = "clinicId";
    public string CookieName { get; set; } = "admin.scope";
    public string Icon { get; set; } = "local_hospital";
}
