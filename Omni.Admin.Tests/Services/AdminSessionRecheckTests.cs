using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Omni.Admin.Business.Contracts.HttpClients.MainApp;
using Omni.Admin.Business.Contracts.Managers;
using Omni.Admin.Business.Contracts.Services.Auth;
using Omni.Admin.Business.Services.Admins;
using Omni.Admin.Business.Services.Auth;
using Omni.Admin.Common.Exceptions;
using Omni.Admin.Common.Statics;
using Omni.Admin.Entities.Dtos.Admins;
using Omni.Admin.Tests.Support;

namespace Omni.Admin.Tests.Services;

/// <summary>The per-request check (cookie OnValidatePrincipal): a deactivated or demoted admin is signed out.</summary>
public class AdminSessionRecheckTests
{
    private readonly Mock<IAuthApiClient> _api = new();
    private readonly Mock<IAuthenticationService> _authentication = new();
    private readonly MemoryCache _cache = TestKit.Cache();

    private (CookieValidatePrincipalContext Context, IServiceProvider Services) Request(ClaimsPrincipal user, IServiceProvider? services = null)
    {
        services ??= new ServiceCollection()
            .AddSingleton<IMemoryCache>(_cache)
            .AddSingleton(_api.Object)
            .AddSingleton<ILogger<AdminAuthService>>(NullLogger<AdminAuthService>.Instance)
            .AddScoped<IAdminAuthService, AdminAuthService>()
            .AddSingleton(_authentication.Object)
            .BuildServiceProvider();
        var http = new DefaultHttpContext { RequestServices = services };
        var scheme = new AuthenticationScheme(CookieAuthenticationDefaults.AuthenticationScheme, null, typeof(CookieAuthenticationHandler));
        var ticket = new AuthenticationTicket(user, scheme.Name);
        return (new CookieValidatePrincipalContext(http, scheme, new CookieAuthenticationOptions(), ticket), services);
    }

    private void VerifySignedOut(Times times) =>
        _authentication.Verify(a => a.SignOutAsync(It.IsAny<HttpContext>(), CookieAuthenticationDefaults.AuthenticationScheme,
            It.IsAny<AuthenticationProperties>()), times);

    [Fact]
    public async Task An_active_admin_keeps_the_cookie()
    {
        _api.Setup(a => a.GetActiveAdminAsync(TestKit.Ada.Id, It.IsAny<CancellationToken>())).ReturnsAsync(TestKit.Ada);
        var (context, _) = Request(TestKit.SignedIn(TestKit.Ada));

        await AdminAuthService.ValidatePrincipalAsync(context);

        Assert.NotNull(context.Principal);
        Assert.False(context.ShouldRenew);
        VerifySignedOut(Times.Never());
    }

    [Fact]
    public async Task A_deactivated_or_demoted_admin_is_signed_out()
    {
        _api.Setup(a => a.GetActiveAdminAsync(TestKit.Ada.Id, It.IsAny<CancellationToken>())).ReturnsAsync((AdminIdentity?)null);
        var (context, _) = Request(TestKit.SignedIn(TestKit.Ada));

        await AdminAuthService.ValidatePrincipalAsync(context);

        Assert.Null(context.Principal);
        VerifySignedOut(Times.Once());
    }

    [Fact]
    public async Task A_cookie_without_an_admin_id_is_signed_out_without_asking()
    {
        var (context, _) = Request(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, "x@omni.test") }, "Cookies")));

        await AdminAuthService.ValidatePrincipalAsync(context);

        Assert.Null(context.Principal);
        VerifySignedOut(Times.Once());
        _api.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task When_the_main_app_cannot_answer_the_cookie_is_kept()
    {
        _api.Setup(a => a.GetActiveAdminAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MainAppApiException("Couldn't reach the main app."));
        var (context, _) = Request(TestKit.SignedIn(TestKit.Ada));

        await AdminAuthService.ValidatePrincipalAsync(context);

        Assert.NotNull(context.Principal);
        VerifySignedOut(Times.Never());
    }

    [Fact]
    public async Task A_changed_name_or_email_renews_the_cookie()
    {
        _api.Setup(a => a.GetActiveAdminAsync(TestKit.Ada.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestKit.Ada with { FullName = "Ada Lovelace", Email = "ada.l@omni.test" });
        var (context, _) = Request(TestKit.SignedIn(TestKit.Ada));

        await AdminAuthService.ValidatePrincipalAsync(context);

        Assert.True(context.ShouldRenew);
        Assert.Equal("Ada Lovelace", AdminClaims.Name(context.Principal!));
        Assert.Equal("ada.l@omni.test", AdminClaims.Email(context.Principal!));
        Assert.Equal(TestKit.Ada.Id, AdminClaims.Id(context.Principal!));
    }

    [Fact]
    public async Task Requests_within_a_minute_ask_the_main_app_once()
    {
        _api.Setup(a => a.GetActiveAdminAsync(TestKit.Ada.Id, It.IsAny<CancellationToken>())).ReturnsAsync(TestKit.Ada);

        for (var i = 0; i < 3; i++) await AdminAuthService.ValidatePrincipalAsync(Request(TestKit.SignedIn(TestKit.Ada)).Context);

        _api.Verify(a => a.GetActiveAdminAsync(TestKit.Ada.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Deactivating_an_admin_in_the_portal_signs_them_out_on_their_next_click()
    {
        var revoked = false;
        _api.Setup(a => a.GetActiveAdminAsync(TestKit.Ada.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => revoked ? null : TestKit.Ada);
        var (first, services) = Request(TestKit.SignedIn(TestKit.Ada));
        await AdminAuthService.ValidatePrincipalAsync(first);
        Assert.NotNull(first.Principal);

        // Another admin deactivates Ada on the Admins page; the main app now says no.
        var adminsApi = new Mock<IAdminsApiClient>();
        adminsApi.Setup(a => a.SetActiveAsync(TestKit.Ada.Id, false, "other", It.IsAny<CancellationToken>()))
            .Callback(() => revoked = true)
            .ReturnsAsync(new AdminRow(TestKit.Ada.Id, TestKit.Ada.Email, TestKit.Ada.FullName, false, false, null, DateTimeOffset.UtcNow));
        var accounts = new AdminAccountService(adminsApi.Object, services.GetRequiredService<IAdminAuthService>(), Mock.Of<IAdminAudit>());
        await accounts.SetActiveAsync(TestKit.Ada.Id, false, "other");

        var (next, _) = Request(TestKit.SignedIn(TestKit.Ada), services);
        await AdminAuthService.ValidatePrincipalAsync(next);

        Assert.Null(next.Principal);
        VerifySignedOut(Times.Once());
    }
}
