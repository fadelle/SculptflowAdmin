using Omni.Admin.Common.Helpers;
using Omni.Admin.Entities.Dtos.Channels;
using Omni.Admin.Entities.Dtos.Members;
using Omni.Admin.Entities.Dtos.Monitoring;
using Omni.Admin.Entities.Responses.Billing;
using Omni.Admin.Entities.Responses.Monitoring;
using Omni.Admin.Entities.Responses.PlatformAdmin;

namespace Omni.Admin.Tests.Helpers;

/// <summary>QA t6-admin-28: the client page's go-live checklist and the conversation page's "why didn't the bot answer".</summary>
public class ClientHelpTests
{
    private static readonly Guid ClientId = Guid.NewGuid();

    private static MemberRow Owner(DateTimeOffset? lastLogin) => new("u1", "owner@x.test", "Owner", Guid.NewGuid(), ClientId, "Client",
        true, false, true, 0, true, false, DateTimeOffset.UtcNow, GoLive.OwnerRole, true, lastLogin, null);

    private static ChannelRow Channel(string status) => new(Guid.NewGuid(), ClientId, "Client", "whatsapp", status, "Shop", null, null,
        true, null, null, null, null, null, DateTimeOffset.UtcNow);

    private static InboxSettingsDto Inbox(bool aiOn, string botStatus, bool planHasAi = true)
    {
        var botId = Guid.NewGuid();
        return new InboxSettingsDto(planHasAi, 1,
            [new AccountAiSettingsDto(Guid.NewGuid(), "whatsapp", "Shop", aiOn, botId, true, true, "Helper", botStatus, null,
                aiOn && botStatus == "active" && planHasAi)],
            [new BotChoiceDto(botId, "Helper", botStatus)]);
    }

    private static ClientBillingSummary Billing(bool access, decimal spendable) => new(true, "USD", "starter", "Starter", 20m, "month",
        access ? "active" : null, access, null, null, false, null, 0m, 0m, spendable, 0m, spendable, [], DateTimeOffset.UtcNow, [], 0m, [], [], []);

    [Fact]
    public void A_ready_client_has_every_step_done()
    {
        var steps = GoLive.Steps(ClientId, true, [Owner(DateTimeOffset.UtcNow)], [Channel("connected")], Inbox(true, "active"), Billing(true, 10m));
        Assert.Equal(6, steps.Count);
        Assert.All(steps, s => Assert.True(s.Done, s.Label));
    }

    [Fact]
    public void A_new_client_shows_what_is_missing_with_a_link_for_each()
    {
        var steps = GoLive.Steps(ClientId, true, [], [Channel("pending")], Inbox(false, "draft"), Billing(false, 0m))
            .ToDictionary(s => s.Label);
        Assert.False(steps["Owner signed in"].Done);
        Assert.Equal("#owner-invite", steps["Owner signed in"].Link);
        Assert.False(steps["Channel connected"].Done);
        Assert.False(steps["AI answering"].Done);
        Assert.False(steps["Plan active"].Done);
        Assert.EndsWith("&open=top-up", steps["Wallet funded"].Link);
        Assert.True(steps["Client active"].Done);
    }

    [Fact]
    public void An_owner_who_never_signed_in_and_unanswered_billing_are_not_done()
    {
        var steps = GoLive.Steps(ClientId, false, [Owner(null)], [], null, null).ToDictionary(s => s.Label);
        Assert.False(steps["Client active"].Done);
        Assert.Contains("hasn't signed in", steps["Owner signed in"].Hint);
        Assert.Contains("didn't answer", steps["Plan active"].Hint);
        Assert.Contains("didn't answer", steps["AI answering"].Hint);
    }

    [Fact]
    public void A_plan_without_ai_points_to_billing()
    {
        var step = GoLive.Steps(ClientId, true, [], [], Inbox(true, "active", planHasAi: false), null).Single(s => s.Label == "AI answering");
        Assert.False(step.Done);
        Assert.StartsWith("/Billing/Client", step.Link);
    }

    private static MessageTrace Trace(string outcome, string? reason, Dictionary<string, string?>? detail = null) =>
        new("t1", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, ClientId, [], [], false,
            [new MessageTraceEntry(DateTimeOffset.UtcNow, "step", "bot.skipped", "skipped", "", null, ClientId, null, null, null, null, detail ?? [])],
            outcome, reason);

    [Theory]
    [InlineData("skipped", "plan_without_ai_assistant", "plan doesn't include", "/Billing/Client")]
    [InlineData("skipped", "no_bot", "No bot is picked", "#account-ai")]
    [InlineData("skipped", "media_message", "can't read", null)]
    [InlineData("no_reply", null, "nothing came back", "/Traces/Details")]
    [InlineData("failed", "send.provider_rejected", "provider rejected", "/Traces/Details")]
    public void Each_outcome_reads_in_plain_words(string outcome, string? reason, string words, string? link)
    {
        var note = BotAnswerWhy.Explain(Trace(outcome, reason), ClientId);
        Assert.Contains(words, note.Headline + " " + note.Detail);
        if (link is null) Assert.Null(note.FixLink);
        else Assert.Contains(link, note.FixLink);
        Assert.False(note.Answered);
    }

    [Fact]
    public void A_conversation_that_started_with_the_team_says_why()
    {
        var note = BotAnswerWhy.Explain(Trace("skipped", "conversation_in_human_mode",
            new Dictionary<string, string?> { ["startedWithTeam"] = "ai_disabled" }), ClientId);
        Assert.Equal("AI is off on this account", note.Headline);
    }

    [Fact]
    public void A_reply_and_a_missing_trace_are_said_too()
    {
        Assert.True(BotAnswerWhy.Explain(Trace("replied", null), ClientId).Answered);
        Assert.Contains("No trace", BotAnswerWhy.Explain(null, ClientId).Headline);
    }
}
