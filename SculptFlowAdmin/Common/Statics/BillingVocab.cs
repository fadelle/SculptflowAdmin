namespace SculptFlowAdmin.Common.Statics;

/// <summary>Vocabularies the main app accepts (validated there too).</summary>
public static class BillingVocab
{
    public static readonly string[] BillingPeriods = ["month", "year"];
    public static readonly string[] BalanceTypes = ["wallet", "included_credit"];
    public static readonly string[] ChargeStatuses = ["reserved", "settled", "released", "failed", "not_charged"];

    /// <summary>Billable event types the main app knows (others can be priced too; these fill the form's suggestions).</summary>
    public static readonly string[] EventTypes =
    [
        "whatsapp_marketing_message", "whatsapp_utility_message", "whatsapp_authentication_message", "whatsapp_service_message",
        "telegram_message", "sms_segment", "viber_transactional_message", "viber_promotional_message", "email_recipient",
        "rcs_message", "voice_minute", "ai_token", "whatsapp_number_month", "agent_seat_month"
    ];
}
