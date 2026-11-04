using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Omni.Admin.Business.HttpClients.MainApp;
using Omni.Admin.Tests.Support;

namespace Omni.Admin.Tests.HttpClients;

/// <summary>QA t7-10: the trace list's outcome, the conversations nobody answered, and AI usage per client.</summary>
public class MessageTracesApiClientTests
{
    private static readonly Guid ClientId = Guid.Parse("c1000000-0000-4000-8000-000000000001");
    private static readonly Guid ConversationId = Guid.Parse("c2000000-0000-4000-8000-000000000002");

    private readonly FakeHttpHandler _http = new();

    private MessageTracesApiClient Client() =>
        new(new HttpClient(_http), TestKit.Options(), TestKit.Accessor(TestKit.SignedIn(TestKit.Ada)), NullLogger<MessageTracesApiClient>.Instance);

    [Fact]
    public async Task The_list_reads_each_traces_outcome_and_reason()
    {
        _http.Enqueue(HttpStatusCode.OK, $$"""
            {"from":"2026-11-04T00:00:00Z","to":"2026-11-05T00:00:00Z","page":1,"pageSize":50,"total":1,"enabled":true,"retentionDays":14,
             "failedRetentionDays":60,"items":[{"traceId":"t1","startedAt":"2026-11-04T10:00:00Z","endedAt":"2026-11-04T10:00:05Z","steps":3,
             "failedSteps":0,"errors":0,"clientId":"{{ClientId}}","conversationId":"{{ConversationId}}","channel":"whatsapp",
             "firstStep":"webhook.received","failedStep":null,"failedDetail":null,"outcome":"skipped","outcomeReason":"message_type_not_supported"}]}
            """);

        var page = await Client().ListAsync(24, null, null, false, 1, 50, default);

        var row = Assert.Single(page.Items);
        Assert.Equal(("skipped", "message_type_not_supported", ConversationId), (row.Outcome, row.OutcomeReason, row.ConversationId!.Value));
    }

    [Fact]
    public async Task Unanswered_asks_for_the_period_and_wait_and_reads_the_rows()
    {
        _http.Enqueue(HttpStatusCode.OK, $$"""
            {"from":"2026-11-04T00:00:00Z","before":"2026-11-04T23:45:00Z","minutesWithoutAnswer":15,"page":1,"pageSize":50,"total":1,
             "items":[{"conversationId":"{{ConversationId}}","clientId":"{{ClientId}}","clientName":"Glow","contactName":"Rana","channel":"whatsapp",
             "mode":"ai","lastCustomerMessageAt":"2026-11-04T10:00:00Z","messageId":null,"messageType":"image","traceId":"t1",
             "outcome":"skipped","outcomeReason":"message_type_not_supported"}]}
            """);

        var page = await Client().UnansweredAsync(24, 15, ClientId, 1, 50, default);

        Assert.Equal($"{TestKit.BaseUrl}/api/platform-admin/message-traces/unanswered?hours=24&minutes=15&clientId={ClientId}&page=1&pageSize=50",
            _http.Requests[0].Uri.ToString());
        Assert.Equal(("Glow", "image", "skipped"), (Assert.Single(page.Items).ClientName, page.Items[0].MessageType, page.Items[0].Outcome));
    }

    [Fact]
    public async Task Ai_usage_reads_the_estimate_and_revenue_per_client()
    {
        _http.Enqueue(HttpStatusCode.OK, $$"""
            {"from":"2026-10-28T00:00:00Z","to":"2026-11-04T00:00:00Z","inputTokenPriceUsdPerMillion":1.25,"outputTokenPriceUsdPerMillion":10,
             "traceRetentionDays":14,"rows":[{"clientId":"{{ClientId}}","clientName":"Glow","runs":49,"failedRuns":3,"modelCalls":136,
             "inputTokens":791000,"outputTokens":12000,"estimatedModelCostUsd":1.10875,"aiReplyRevenue":0.06,"revenueCurrency":"USD"}]}
            """);

        var report = await Client().AiUsageAsync(7, null, default);

        Assert.Equal($"{TestKit.BaseUrl}/api/platform-admin/message-traces/ai-usage?days=7", _http.Requests[0].Uri.ToString());
        var row = Assert.Single(report.Rows);
        Assert.Equal((136L, 791000L, 1.10875m, 0.06m), (row.ModelCalls, row.InputTokens, row.EstimatedModelCostUsd, row.AiReplyRevenue));
    }
}
