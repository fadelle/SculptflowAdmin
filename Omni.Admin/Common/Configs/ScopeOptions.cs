namespace Omni.Admin.Common.Configs;

/// <summary>Names of the global scope (config section "Scope", optional), so "Client" → something else later is
/// configuration, not a rewrite. QueryKey is the pages' existing client filter parameter.</summary>
public sealed class ScopeOptions
{
    public string EntityLabel { get; set; } = "Client";
    public string EntityLabelPlural { get; set; } = "Clients";
    public string QueryKey { get; set; } = "clientId";
    public string CookieName { get; set; } = "admin.scope";
    public string Icon { get; set; } = "domain";
}
