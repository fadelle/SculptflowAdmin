using System.Globalization;
using Microsoft.AspNetCore.Mvc;

namespace SculptFlowAdmin.Common.Helpers;

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

    /// <summary>"whatsapp_marketing_message" → "WhatsApp marketing message".</summary>
    public static string EventLabel(string eventType)
    {
        var text = eventType.Replace('_', ' ');
        text = text.StartsWith("whatsapp", StringComparison.Ordinal) ? "WhatsApp" + text["whatsapp".Length..] : text;
        text = text.Replace(" ai ", " AI ").Replace("sms", "SMS");
        return text.Length == 0 ? text : char.ToUpperInvariant(text[0]) + text[1..];
    }

    /// <summary>Admin label of a provider billing responsibility (the main app's ProviderBillingResponsibility.AdminLabel).</summary>
    public static string ResponsibilityLabel(string? value) => value switch
    {
        null or "" => "Any",
        "customer_direct" => "Customer pays provider directly",
        "platform_funded" => "SculptFlow pays provider",
        "external_provider_direct" => "Customer pays external provider",
        "no_provider_usage_fee" => "No provider usage fee",
        _ => value
    };

    public static string BalanceLabel(string balanceType) => balanceType == "included_credit" ? "Included credit" : "Wallet";
}
