using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Omni.Admin.Business.HttpClients.MainApp;
using Omni.Admin.Common.Exceptions;
using Omni.Admin.Tests.Support;

namespace Omni.Admin.Tests.HttpClients;

public class AdminsApiClientTests
{
    private const string Row = """{"id":"ad1","email":"ada@omni.test","fullName":"Ada Admin","isActive":true,"loginLocked":false,"lastLoginAt":null,"createdAt":"2026-10-20T07:00:00+00:00"}""";

    private readonly FakeHttpHandler _http = new();

    private AdminsApiClient Client() =>
        new(new HttpClient(_http), TestKit.Options(), TestKit.Accessor(TestKit.SignedIn(TestKit.Ada)), NullLogger<AdminsApiClient>.Instance);

    [Fact]
    public async Task List_reads_every_admin_with_the_signed_in_admin_as_actor()
    {
        _http.Enqueue(HttpStatusCode.OK, $"[{Row}]");

        var admins = await Client().ListAsync(default);

        var admin = Assert.Single(admins);
        Assert.Equal("ad1", admin.Id);
        Assert.True(admin.IsActive);
        Assert.False(admin.LoginLocked);
        Assert.Equal($"{TestKit.BaseUrl}/api/platform-admin/admins", _http.Requests[0].Uri.ToString());
        Assert.Equal("ada@omni.test", _http.Requests[0].Headers["X-Admin-Actor"]);
    }

    [Fact]
    public async Task Create_posts_email_name_and_password()
    {
        _http.Enqueue(HttpStatusCode.Created, Row);

        var admin = await Client().CreateAsync("ada@omni.test", "Ada Admin", "long-enough-pw", default);

        Assert.Equal("ad1", admin.Id);
        Assert.Equal(HttpMethod.Post, _http.Requests[0].Method);
        using var body = JsonDocument.Parse(_http.Requests[0].Body!);
        Assert.Equal("ada@omni.test", body.RootElement.GetProperty("email").GetString());
        Assert.Equal("Ada Admin", body.RootElement.GetProperty("fullName").GetString());
        Assert.Equal("long-enough-pw", body.RootElement.GetProperty("password").GetString());
    }

    [Fact]
    public async Task SetActive_sends_the_acting_admin_so_the_main_app_can_refuse_your_own_account()
    {
        _http.Enqueue(HttpStatusCode.OK, Row);

        await Client().SetActiveAsync("ad2", false, "ad1", default);

        Assert.Equal(HttpMethod.Put, _http.Requests[0].Method);
        Assert.Equal($"{TestKit.BaseUrl}/api/platform-admin/admins/ad2/active", _http.Requests[0].Uri.ToString());
        using var body = JsonDocument.Parse(_http.Requests[0].Body!);
        Assert.False(body.RootElement.GetProperty("active").GetBoolean());
        Assert.Equal("ad1", body.RootElement.GetProperty("actingAdminId").GetString());
    }

    [Fact]
    public async Task SetPassword_puts_the_new_password()
    {
        _http.Enqueue(HttpStatusCode.OK, Row);

        await Client().SetPasswordAsync("ad1", "new-long-password", default);

        Assert.Equal($"{TestKit.BaseUrl}/api/platform-admin/admins/ad1/password", _http.Requests[0].Uri.ToString());
        using var body = JsonDocument.Parse(_http.Requests[0].Body!);
        Assert.Equal("new-long-password", body.RootElement.GetProperty("password").GetString());
    }

    [Fact]
    public async Task A_refused_change_surfaces_the_main_apps_message()
    {
        _http.Enqueue(HttpStatusCode.BadRequest, """{"error":"At least one admin must stay active."}""");

        var ex = await Assert.ThrowsAsync<MainAppApiException>(() => Client().SetActiveAsync("ad1", false, "ad2", default));
        Assert.Equal("At least one admin must stay active.", ex.Message);
    }

    [Fact]
    public async Task A_missing_admin_is_not_found()
    {
        _http.Enqueue(HttpStatusCode.NotFound);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => Client().SetPasswordAsync("gone", "new-long-password", default));
    }

    [Fact]
    public async Task Create_on_a_main_app_with_the_api_off_says_so()
    {
        _http.Enqueue(HttpStatusCode.NotFound);

        var ex = await Assert.ThrowsAsync<MainAppApiException>(() => Client().CreateAsync("a@b.test", "A", "long-enough-pw", default));
        Assert.Contains("off", ex.Message);
    }
}
