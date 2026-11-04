using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Omni.Admin.Business.HttpClients.MainApp;
using Omni.Admin.Entities.Requests.Clients;
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

    [Fact]
    public async Task Create_posts_the_new_client_and_reads_what_happened()
    {
        // QA t6-admin-20.
        _http.Enqueue(HttpStatusCode.Created, $$"""{"clientId":"{{ClientId}}","slug":"glow","planCode":"growth","planStarted":true,"inviteSent":false,"inviteExpiresAt":null}""");

        var created = await Client().CreateAsync(new CreateClientRequest("Glow", "clinic", "olga@x.com", "Olga", "growth", "ar"), default);

        Assert.Equal((ClientId, "glow", "growth", true, false), (created.ClientId, created.Slug, created.PlanCode, created.PlanStarted, created.InviteSent));
        Assert.Equal(HttpMethod.Post, _http.Requests[0].Method);
        Assert.Equal($"{TestKit.BaseUrl}/api/platform-admin/clients", _http.Requests[0].Uri.ToString().TrimEnd('/'));
        using var body = JsonDocument.Parse(_http.Requests[0].Body!);
        Assert.Equal(("Glow", "clinic", "olga@x.com", "growth", "ar"), (body.RootElement.GetProperty("name").GetString(), body.RootElement.GetProperty("businessTypeCode").GetString(),
            body.RootElement.GetProperty("ownerEmail").GetString(), body.RootElement.GetProperty("planCode").GetString(), body.RootElement.GetProperty("language").GetString()));
    }

    [Fact]
    public async Task ResendOwnerInvite_posts_to_the_clients_owner_invite()
    {
        _http.Enqueue(HttpStatusCode.OK, $$"""{"clientId":"{{ClientId}}","email":"olga@x.com","expiresAt":"2026-11-11T12:00:00+00:00"}""");

        var sent = await Client().ResendOwnerInviteAsync(ClientId, default);

        Assert.Equal("olga@x.com", sent.Email);
        Assert.Equal($"{TestKit.BaseUrl}/api/platform-admin/clients/{ClientId}/owner-invite", _http.Requests[0].Uri.ToString());
        Assert.Equal(HttpMethod.Post, _http.Requests[0].Method);
    }
}
