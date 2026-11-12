using System.Globalization;
using Microsoft.AspNetCore.Mvc;

namespace Omni.Admin.Common.Helpers;

/// <summary>Formatting shared by the billing pages.</summary>
public static class BillingUi
{
    /// <summary>An amount with its currency, up to 4 decimals (rates are often fractions of a cent).</summary>
    public static string Money(decimal? amount, string? currency = null)
    {
        if (amount is null) return "—";
        var text = amount.Value.ToString("#,##0.00##", CultureInfo.InvariantCulture);
        return string.IsNullOrEmpty(currency) ? text : $"{text} {currency}";
    }

    public static string Number(decimal value) => value.ToString("#,##0.####", CultureInfo.InvariantCulture);

    /// <summary>Invariant decimal for an input's value attribute.</summary>
    public static string Input(decimal value) => value.ToString(CultureInfo.InvariantCulture);

    /// <summary>The top-up dialog's one-click amounts (QA t6-admin-28).</summary>
    public static readonly int[] QuickTopUps = [10, 25, 50, 100, 250];

    /// <summary>The overview's third number (QA t6-admin-29): when the client is next charged, or when its plan ends, in plain words.</summary>
    public static (string Label, DateTimeOffset? At, string? Note) NextCharge(Entities.Responses.Billing.ClientBillingSummary s) =>
        s.SubscriptionStatus switch
        {
            null or "" or "none" => ("Next charge", null, "no plan yet"),
            "expired" or "cancelled" => ("Plan ended", s.EndedAt ?? s.CurrentPeriodEnd, "start a plan to charge again"),
            "past_due" => ("Renewal unpaid", s.GraceEndsAt ?? s.CurrentPeriodEnd,
                s.GraceEndsAt is null ? "top up the wallet so it renews" : "features stop then unless the wallet is topped up"),
            _ when s.CancelAtPeriodEnd => ("Plan ends on", s.CurrentPeriodEnd, "cancelled at period end: no next charge"),
            _ when s.TrialDays is > 0 => ("Trial ends on", s.CurrentPeriodEnd, "it ends instead of renewing: start a paid plan before then"),
            _ => ("Next charge on", s.CurrentPeriodEnd, s.PlanPrice is null ? null : $"{Money(s.PlanPrice, s.Currency)} from the wallet")
        };

    /// <summary>"whatsapp_marketing_message" → "WhatsApp marketing message", "ai_reply" → "AI reply".</summary>
    public static string EventLabel(string eventType)
    {
        var text = eventType.Replace('_', ' ');
        text = text.StartsWith("whatsapp", StringComparison.Ordinal) ? "WhatsApp" + text["whatsapp".Length..] : text;
        text = text.StartsWith("ai ", StringComparison.Ordinal) ? "AI" + text[2..] : text;
        text = text.Replace(" ai ", " AI ").Replace("sms", "SMS");
        return text.Length == 0 ? text : char.ToUpperInvariant(text[0]) + text[1..];
    }

    /// <summary>Admin label of a provider billing responsibility (the main app's ProviderBillingResponsibility.AdminLabel).</summary>
    public static string ResponsibilityLabel(string? value) => value switch
    {
        null or "" => "Any",
        "customer_direct" => "Customer pays provider directly",
        "platform_funded" => "Omni pays provider",
        "external_provider_direct" => "Customer pays external provider",
        "no_provider_usage_fee" => "No provider usage fee",
        _ => value
    };

    public static string BalanceLabel(string balanceType) => balanceType == "included_credit" ? "Included credit" : "Wallet";

    /// <summary>What "Run renewal now" did, from the main app's outcome (QA t6-admin-21: it always said "Renewal check ran").</summary>
    public static string RenewOutcome(string outcome) => outcome switch
    {
        "Renewed" => "Renewed: the new period started and its price was charged.",
        "NotDue" => "Nothing to do: the current period hasn't ended yet.",
        "PastDue" => "Couldn't renew: the wallet doesn't cover the plan. The subscription is past due; top up and run it again.",
        "Expired" => "The subscription has expired (a trial ended, or the grace period ran out unpaid).",
        "Cancelled" => "The subscription ended: it was set to cancel at the period's end.",
        _ => "Renewal check ran. The subscription below shows the outcome."
    };

    /// <summary>" Now over the plan: Seats 3 of 1, AI bots 1 of 0. Deactivate what's extra or add units." after a plan change
    /// that leaves the client over its limits (QA t2-billing-11); empty when everything fits.</summary>
    public static string OverLimitsNote(IReadOnlyList<Entities.Responses.Billing.OverLimitRow>? over) =>
        over is not { Count: > 0 } ? ""
            : " Now over the plan: " + string.Join(", ", over.Select(o => $"{LimitName(o.Key)} {o.Used} of {o.Limit}"))
              + ". Deactivate what's extra (members, channels, bots) or add units below.";

    private static string LimitName(string key) => key switch
    {
        "max_users" => "Seats",
        "max_channel_connections" => "Channels",
        "max_whatsapp_numbers" => "WhatsApp numbers",
        "max_ai_bots" => "AI bots",
        _ => key
    };
}
