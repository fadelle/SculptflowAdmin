using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using SculptFlowAdmin.Business.Contracts.Services.Billing;
using SculptFlowAdmin.Business.Mappers.Billing;
using SculptFlowAdmin.Entities.Dtos.Billing;
using SculptFlowAdmin.Entities.Responses.Billing;
using SculptFlowAdmin.Pages.Shared;

namespace SculptFlowAdmin.Pages.Billing;

/// <summary>Revenue report: usage revenue, provider cost (paid by SculptFlow vs externally), margin, subscriptions, top-ups.</summary>
public class ReportModel : MainAppPageModel
{
    private readonly IBillingAdminService _billing;

    public ReportModel(IBillingAdminService billing) => _billing = billing;

    /// <summary>yyyy-MM-dd, UTC; default = the current month.</summary>
    [BindProperty(SupportsGet = true)] public string? From { get; set; }
    [BindProperty(SupportsGet = true)] public string? To { get; set; }
    [BindProperty(SupportsGet = true)] public string GroupBy { get; set; } = "clinic";
    public BillingReport? Report { get; private set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var from = ParseDate(From) ?? monthStart;
        // "To" is inclusive in the form (a date), exclusive in the API.
        var to = (ParseDate(To) ?? from.AddMonths(1).AddDays(-1)).AddDays(1);
        if (to <= from) to = from.AddDays(1);
        From = from.ToString("yyyy-MM-dd");
        To = to.AddDays(-1).ToString("yyyy-MM-dd");
        GroupBy = GroupBy is "channel" or "event" or "who_pays" ? GroupBy : "clinic";
        await LoadAsync(async () => Report = await _billing.ReportAsync(from, to, ct));
    }

    public List<BillingReportGroup> Groups() => BillingReportGrouping.Group(Report, GroupBy);

    private static DateTimeOffset? ParseDate(string? value) =>
        DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var d)
            ? new DateTimeOffset(d, TimeSpan.Zero) : null;
}
