using SculptFlowAdmin.Entities.Dtos.Navigation;

namespace SculptFlowAdmin.Common.Statics;

/// <summary>
/// The ONE navigation tree: the sidebar, breadcrumb, section tabs and Ctrl+K palette all read it (docs/UI_GUIDE.md §6).
/// Adding a page = one line here. Paths are Razor Page routes (no PathBase). A group of leaves is one sidebar row whose
/// pages become the header tabs.
/// </summary>
public static class NavTree
{
    public static readonly NavSection[] Sections =
    {
        // Captioned (unlike the guide's untitled first section) so Dashboard never reads as part of Favorites above it.
        new("Overview", new[] { new NavItem("Dashboard", "dashboard", "/", Exact: true, Aliases: new[] { "/Index" }) }), // "/" MUST be Exact
        new("Operations", new[]
        {
            new NavItem("Patients", "groups", Children: new[]
            {
                new NavItem("Leads", "person_search", "/Leads"),
                new NavItem("Conversations", "forum", "/Conversations"),
                new NavItem("Messages", "send", "/Messages"),
                new NavItem("Appointments", "event", "/Appointments"),
            }),
            new NavItem("Logs", "receipt_long", Children: new[]
            {
                new NavItem("Event log", "monitor_heart", "/Events"),
                new NavItem("Admin audit log", "verified_user", "/Audit"),
            }),
        }),
        new("Manage", new[]
        {
            new NavItem("Tenants", "domain", Children: new[]
            {
                new NavItem("Clinics", "local_hospital", "/Clinics"),
                new NavItem("Staff accounts", "badge", "/Staff"),
            }),
            new NavItem("Billing", "payments", Children: new[]
            {
                new NavItem("Billing accounts", "account_balance_wallet", "/Billing", Exact: true,
                    Aliases: new[] { "/Billing/Index", "/Billing/Clinic" }),
                new NavItem("Plans", "layers", "/Billing/Plans"),
                new NavItem("Rate cards", "request_quote", "/Billing/RateCards", Aliases: new[] { "/Billing/Rates" }),
                new NavItem("Revenue report", "monitoring", "/Billing/Report"),
            }),
            new NavItem("Setup", "tune", Children: new[]
            {
                new NavItem("Channels & integrations", "hub", "/Channels"),
                new NavItem("WhatsApp templates", "article", "/Templates"),
                new NavItem("Campaigns", "campaign", "/Campaigns"),
                new NavItem("Procedures", "medical_services", "/Procedures"),
                new NavItem("Knowledge base", "menu_book", "/Knowledge"),
            }),
            new NavItem("System", "settings", Children: new[]
            {
                new NavItem("Configuration", "settings_suggest", "/Configuration"),
                new NavItem("Cache", "storage", "/Cache"),
                new NavItem("Admin users", "admin_panel_settings", "/Admins"),
            }),
        }),
    };

    public static readonly NavItem[] TopLevel = Sections.SelectMany(s => s.Items).ToArray();

    /// <summary>Root→leaf chain for a request path; each step carries its siblings. Empty when the path isn't in the tree.</summary>
    public static IReadOnlyList<NavStep> Resolve(string path) => Find(TopLevel, path) ?? new List<NavStep>();

    /// <summary>Every leaf with its trail (section and group labels) — feeds the Ctrl+K palette.</summary>
    public static IEnumerable<(NavItem Item, string[] Trail)> Leaves()
    {
        foreach (var section in Sections)
            foreach (var item in section.Items)
                foreach (var leaf in Walk(item, section.Title is null ? Array.Empty<string>() : new[] { section.Title }))
                    yield return leaf;
    }

    private static List<NavStep>? Find(NavItem[] items, string path)
    {
        foreach (var item in items)
        {
            if (item.Matches(path)) return new List<NavStep> { new(item, items) };
            if (item.Children is { Length: > 0 } children && Find(children, path) is { } sub)
            {
                sub.Insert(0, new NavStep(item, items));
                return sub;
            }
        }
        return null;
    }

    private static IEnumerable<(NavItem, string[])> Walk(NavItem item, string[] trail)
    {
        if (item.IsLeaf) { yield return (item, trail); yield break; }
        foreach (var child in item.Children ?? Array.Empty<NavItem>())
            foreach (var leaf in Walk(child, trail.Append(item.Label).ToArray()))
                yield return leaf;
    }
}
