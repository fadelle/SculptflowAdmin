using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Omni.Admin.Business.HttpClients.MainApp;
using Omni.Admin.Tests.Support;

namespace Omni.Admin.Tests.HttpClients;

public class MembersApiClientTests
{
    private readonly FakeHttpHandler _http = new();

    private MembersApiClient Client() =>
        new(new HttpClient(_http), TestKit.Options(), TestKit.Accessor(TestKit.SignedIn(TestKit.Ada)), NullLogger<MembersApiClient>.Instance);

    private static string Row(string id, string language) => $$"""
        {"userId":"{{id}}","email":"{{id}}@omni.test","fullName":null,"membershipId":null,"clientId":null,"clientName":null,
         "membershipActive":true,"loginLocked":false,"emailConfirmed":true,"accessFailedCount":0,"hasPassword":true,
         "hasGoogleLogin":false,"joinedAt":null,"role":"member","userActive":true,"lastLoginAt":null,"language":{{language}}}
        """;

    [Fact]
    public async Task List_reads_each_users_own_language_or_none()
    {
        _http.Enqueue(HttpStatusCode.OK, $$"""{"items":[{{Row("u1", "\"ar\"")}},{{Row("u2", "null")}}],"page":1,"pageSize":50,"total":2}""");

        var result = await Client().ListAsync(null, null, 1, default);

        Assert.Equal("ar", result.Items[0].Language);
        Assert.Null(result.Items[1].Language);
    }

    [Fact]
    public async Task Reset_emails_and_invites_use_the_members_api()
    {
        var clientId = Guid.NewGuid();
        var inviteId = Guid.NewGuid();
        _http.Enqueue(HttpStatusCode.OK, $$"""{"clientId":"{{clientId}}"}""");
        _http.Enqueue(HttpStatusCode.OK, $$"""[{"id":"{{inviteId}}","email":"ann@x.com","fullName":null,"createdAt":"2026-10-21T10:00:00Z","expiresAt":"2026-10-28T10:00:00Z","isExpired":false}]""");
        _http.Enqueue(HttpStatusCode.OK, $$"""{"clientId":"{{clientId}}"}""");

        Assert.Equal(clientId, (await Client().SendPasswordResetEmailAsync("u 1", default)).ClientId);
        var invite = Assert.Single(await Client().ListInvitesAsync(clientId, default));
        Assert.Equal((inviteId, "ann@x.com", false), (invite.Id, invite.Email, invite.IsExpired));
        Assert.Equal(clientId, (await Client().RevokeInviteAsync(inviteId, default)).ClientId);

        Assert.Equal((HttpMethod.Post, "/api/platform-admin/members/u%201/password-reset-email"), (_http.Requests[0].Method, _http.Requests[0].Uri.AbsolutePath));
        Assert.Equal($"?clientId={clientId}", _http.Requests[1].Uri.Query);
        Assert.Equal((HttpMethod.Delete, $"/api/platform-admin/members/invites/{inviteId}"), (_http.Requests[2].Method, _http.Requests[2].Uri.AbsolutePath));
    }
}
