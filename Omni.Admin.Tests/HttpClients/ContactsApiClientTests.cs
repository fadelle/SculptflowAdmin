using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Omni.Admin.Business.HttpClients.MainApp;
using Omni.Admin.Tests.Support;

namespace Omni.Admin.Tests.HttpClients;

/// <summary>Milestone C P2: a contact's data export (passed through as JSON) and delete.</summary>
public class ContactsApiClientTests
{
    private readonly FakeHttpHandler _http = new();

    private ContactsApiClient Client() =>
        new(new HttpClient(_http), TestKit.Options(), TestKit.Accessor(TestKit.SignedIn(TestKit.Ada)), NullLogger<ContactsApiClient>.Instance);

    [Fact]
    public async Task Export_passes_the_main_apps_json_through_and_delete_answers_the_client()
    {
        var id = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        _http.Enqueue(HttpStatusCode.OK, """{"contact":{"fullName":"رنا","phone":"+961"},"messages":[{"content":"hi"}],"futureField":1}""");
        _http.Enqueue(HttpStatusCode.NotFound);
        _http.Enqueue(HttpStatusCode.OK, $$"""{"clientId":"{{clientId}}"}""");

        var json = await Client().ExportContactAsync(id, default);
        var missing = await Client().ExportContactAsync(Guid.NewGuid(), default);
        var change = await Client().DeleteContactAsync(id, default);

        Assert.Contains("\"futureField\": 1", json);  // nothing dropped, indented for the download
        Assert.Contains("\"content\": \"hi\"", json);
        Assert.Contains("\"fullName\": \"رنا\"", json);  // readable, not \u-escaped
        Assert.Contains("\"phone\": \"+961\"", json);
        Assert.Null(missing);
        Assert.Equal(clientId, change.ClientId);
        Assert.Equal($"/api/platform-admin/contacts/{id}/export", _http.Requests[0].Uri.AbsolutePath);
        Assert.Equal((HttpMethod.Delete, $"/api/platform-admin/contacts/{id}"), (_http.Requests[2].Method, _http.Requests[2].Uri.AbsolutePath));
    }
}
