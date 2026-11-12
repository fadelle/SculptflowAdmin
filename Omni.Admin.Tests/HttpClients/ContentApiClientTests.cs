using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Omni.Admin.Business.HttpClients.MainApp;
using Omni.Admin.Tests.Support;

namespace Omni.Admin.Tests.HttpClients;

/// <summary>QA t4-ai-15: the client page starts a Knowledge Base re-index and reads its progress.</summary>
public class ContentApiClientTests
{
    private readonly FakeHttpHandler _http = new();

    private ContentApiClient Client() =>
        new(new HttpClient(_http), TestKit.Options(), TestKit.Accessor(TestKit.SignedIn(TestKit.Ada)), NullLogger<ContentApiClient>.Instance);

    [Fact]
    public async Task A_reindex_is_started_and_its_progress_read()
    {
        var clientId = Guid.NewGuid();
        _http.Enqueue(HttpStatusCode.Accepted, $$"""{"clientId":"{{clientId}}"}""");
        _http.Enqueue(HttpStatusCode.OK, """{"total":40,"done":12,"failed":1,"startedAt":"2026-11-07T09:00:00Z","finishedAt":null,"running":true}""");
        _http.Enqueue(HttpStatusCode.NotFound);

        var change = await Client().StartReindexAsync(clientId, default);
        var progress = await Client().ReindexProgressAsync(clientId, default);
        var none = await Client().ReindexProgressAsync(clientId, default);

        Assert.Equal(clientId, change.ClientId);
        Assert.Equal((HttpMethod.Post, $"/api/platform-admin/content/knowledge/reindex/{clientId}"), (_http.Requests[0].Method, _http.Requests[0].Uri.AbsolutePath));
        Assert.Equal((40, 12, 1, true), (progress!.Total, progress.Done, progress.Failed, progress.Running));
        Assert.Null(none);
    }
}
