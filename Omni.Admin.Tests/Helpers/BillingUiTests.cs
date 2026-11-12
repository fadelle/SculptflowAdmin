using Omni.Admin.Common.Helpers;
using Omni.Admin.Common.Statics;

namespace Omni.Admin.Tests.Helpers;

public class BillingUiTests
{
    [Theory]
    [InlineData("ai_reply", "AI reply")]
    [InlineData("ai_token", "AI token")]
    [InlineData("whatsapp_marketing_message", "WhatsApp marketing message")]
    [InlineData("sms_segment", "SMS segment")]
    [InlineData("telegram_message", "Telegram message")]
    public void Event_types_read_as_labels(string eventType, string label) =>
        Assert.Equal(label, BillingUi.EventLabel(eventType));

    [Fact]
    public void The_rate_form_suggests_ai_reply() =>
        Assert.Contains("ai_reply", BillingVocab.EventTypes);

    [Theory]
    [InlineData("Renewed", "Renewed")]
    [InlineData("NotDue", "hasn't ended")]
    [InlineData("PastDue", "past due")]
    [InlineData("Expired", "expired")]
    [InlineData("", "Renewal check ran")]
    public void Run_renewal_now_says_what_happened(string outcome, string expected) =>
        // QA t6-admin-21: it always said "Renewal check ran".
        Assert.Contains(expected, BillingUi.RenewOutcome(outcome));

    private static Omni.Admin.Entities.Responses.Billing.ClientBillingSummary Summary(string? status, bool cancelAtEnd = false,
        int? trialDays = null, DateTimeOffset? graceEnds = null) =>
        new(true, "USD", "starter", "Starter", 20m, "month", status, status is "active" or "past_due", DateTimeOffset.UtcNow.AddDays(-3),
            new DateTimeOffset(2026, 12, 1, 0, 0, 0, TimeSpan.Zero), cancelAtEnd, graceEnds, 5m, 4m, 10m, 0m, 14m, [], DateTimeOffset.UtcNow,
            [], 0m, [], [], [], trialDays);

    [Theory]
    [InlineData("active", false, null, "Next charge on", "20.00 USD from the wallet")]
    [InlineData("active", true, null, "Plan ends on", "no next charge")]
    [InlineData("active", false, 14, "Trial ends on", "start a paid plan")]
    [InlineData("past_due", false, null, "Renewal unpaid", "top up")]
    [InlineData("expired", false, null, "Plan ended", "start a plan")]
    [InlineData(null, false, null, "Next charge", "no plan yet")]
    public void The_overview_says_when_the_client_is_next_charged(string? status, bool cancelAtEnd, int? trialDays, string label, string note)
    {
        // QA t6-admin-29: three plain numbers instead of ledger words.
        var next = BillingUi.NextCharge(Summary(status, cancelAtEnd, trialDays));
        Assert.Equal(label, next.Label);
        Assert.Contains(note, next.Note);
    }

    [Theory]
    [InlineData("whatsapp", "WhatsApp")]
    [InlineData("facebook", "Messenger")]
    [InlineData("telegram", "Telegram")]
    [InlineData("other", "other")]
    public void Channels_read_as_people_say_them(string code, string name) =>
        // QA t6-admin-24: the clients table lists every channel, not a WhatsApp-only column.
        Assert.Equal(name, Ui.ChannelName(code));

    [Fact]
    public void A_plan_change_that_leaves_the_client_over_its_limits_says_so()
    {
        // QA t2-billing-11.
        Assert.Equal("", BillingUi.OverLimitsNote(null));
        Assert.Equal("", BillingUi.OverLimitsNote([]));
        var note = BillingUi.OverLimitsNote([new Omni.Admin.Entities.Responses.Billing.OverLimitRow("max_users", 3, 1), new("max_ai_bots", 1, 0)]);
        Assert.Contains("Seats 3 of 1, AI bots 1 of 0", note);
    }
}
