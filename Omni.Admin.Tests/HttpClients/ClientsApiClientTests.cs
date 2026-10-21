using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Omni.Admin.Business.HttpClients.MainApp;
using Omni.Admin.Tests.Support;

namespace Omni.Admin.Tests.HttpClients;

public class ClientsApiClientTests
{
    private static readonly Guid ClientId = Guid.Parse("c1000000-0000-4000-8000-000000000001");

    private readonly FakeHttpHandler _http = new();

    private ClientsApiClient Client() =>
        new(new HttpClient(_http), TestKit.Options(), TestKit.Accessor(TestKit.SignedIn(TestKit.Ada)), NullLogger<ClientsApiClient>.Instance);

    [Fact]
    public async Task SetDefaultLanguage_puts_the_language_and_reads_the_client_it_changed()
    {
        _http.Enqueue(HttpStatusCode.OK, $$"""{"clientId":"{{ClientId}}"}""");

        var change = await Client().SetDefaultLanguageAsync(ClientId, "ar", default);

        Assert.Equal(ClientId, change.ClientId);
        Assert.Equal(HttpMethod.Put, _http.Requests[0].Method);
        Assert.Equal($"{TestKit.BaseUrl}/api/platform-admin/clients/{ClientId}/default-language", _http.Requests[0].Uri.ToString());
        Assert.Equal("ada@omni.test", _http.Requests[0].Headers["X-Admin-Actor"]);
        using var body = JsonDocument.Parse(_http.Requests[0].Body!);
        Assert.Equal("ar", body.RootElement.GetProperty("language").GetString());
    }

    [Fact]
    public async Task Get_reads_the_clients_default_language()
    {
        _http.Enqueue(HttpStatusCode.OK, $$"""
            {"id":"{{ClientId}}","name":"Demo","slug":"demo","phone":null,"email":null,"website":null,"countryCode":null,
             "timezone":"UTC","address":null,"operatingHours":null,"bookingInfo":null,"isActive":true,
             "createdAt":"2026-10-20T07:00:00+00:00","updatedAt":"2026-10-20T07:00:00+00:00",
             "businessTypeCode":"general","defaultLanguage":"fr"}
            """);

        var client = await Client().GetAsync(ClientId, default);

        Assert.Equal("fr", client!.DefaultLanguage);
    }
}
