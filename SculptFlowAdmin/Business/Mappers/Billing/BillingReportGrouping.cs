using SculptFlowAdmin.Common.Helpers;
using SculptFlowAdmin.Entities.Dtos.Billing;
using SculptFlowAdmin.Entities.Responses.Billing;

namespace SculptFlowAdmin.Business.Mappers.Billing;

public static class BillingReportGrouping
{
    /// <summary>Groups the report's usage rows by "clinic" (default), "channel", "event" or "who_pays", highest revenue first.</summary>
    public static List<BillingReportGroup> Group(BillingReport? report, string groupBy)
    {
        if (report is null) return new();
        IEnumerable<IGrouping<string, BillingReportRow>> grouped = groupBy switch
        {
            "channel" => report.Usage.GroupBy(r => r.Channel),
            "event" => report.Usage.GroupBy(r => r.EventType),
            "who_pays" => report.Usage.GroupBy(r => r.ProviderBilling),
            _ => report.Usage.GroupBy(r => r.ClinicId.ToString())
        };
        return grouped.Select(g =>
        {
            var first = g.First();
            var label = groupBy switch
            {
                "channel" => first.Channel,
                "event" => BillingUi.EventLabel(first.EventType),
                "who_pays" => BillingUi.ResponsibilityLabel(first.ProviderBilling),
                _ => first.ClinicName
            };
            return new BillingReportGroup(g.Key, label, groupBy == "clinic" ? first.ClinicId : null, g.Sum(r => r.Count), g.Sum(r => r.Quantity),
                g.Sum(r => r.Revenue), g.Sum(r => r.ProviderCost), g.Sum(r => r.Margin));
        }).OrderByDescending(g => g.Revenue).ThenBy(g => g.Label).ToList();
    }
}
