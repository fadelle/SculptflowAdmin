using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Omni.Admin.Business.HttpClients.MainApp;
using Omni.Admin.Tests.Support;

namespace Omni.Admin.Tests.HttpClients;

/// <summary>QA t8-20: "Check connection" checks the stored WhatsApp token through the main app.</summary>
public class ChannelsApiClientTests
{
    private readonly FakeHttpHandler _http = new();

    private ChannelsApiClient Client() =>
        new(new HttpClient(_http), TestKit.Options(), TestKit.Accessor(TestKit.SignedIn(TestKit.Ada)), NullLogger<ChannelsApiClient>.Instance);

    [Fact]
    public async Task The_check_posts_to_the_connection_and_reads_metas_answer()
    {
        var id = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        _http.Enqueue(HttpStatusCode.OK, $$"""{"ok":false,"displayPhoneNumber":null,"verifiedName":null,"error":"Session has expired.","checkedAt":"2026-11-07T09:00:00Z","clientId":"{{clientId}}"}""");

        var result = await Client().CheckAsync(id, default);

        Assert.Equal((HttpMethod.Post, $"/api/platform-admin/channels/{id}/check"), (_http.Requests[0].Method, _http.Requests[0].Uri.AbsolutePath));
        Assert.Equal((false, "Session has expired.", clientId), (result.Ok, result.Error, result.ClientId));
    }
}
