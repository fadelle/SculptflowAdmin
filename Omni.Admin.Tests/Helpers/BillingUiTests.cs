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
}
