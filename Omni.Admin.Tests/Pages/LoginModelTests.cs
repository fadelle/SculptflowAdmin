using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Omni.Admin.Business.Contracts.Managers;
using Omni.Admin.Business.Contracts.Services.Auth;
using Omni.Admin.Business.Services.Auth;
using Omni.Admin.Entities.Dtos.Admins;
using Omni.Admin.Pages;
using Omni.Admin.Tests.Support;

namespace Omni.Admin.Tests.Pages;

public class LoginModelTests
{
    private readonly Mock<IAdminAuthService> _auth = new();
    private readonly Mock<IAdminAudit> _audit = new();
    private readonly Mock<IAuthenticationService> _authentication = new();
    private ClaimsPrincipal? _signedIn;

    private LoginModel Page(string? returnUrl = null)
    {
        _authentication.Setup(a => a.SignInAsync(It.IsAny<HttpContext>(), CookieAuthenticationDefaults.AuthenticationScheme,
                It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()))
            .Callback<HttpContext, string, ClaimsPrincipal, AuthenticationProperties>((_, _, p, _) => _signedIn = p)
            .Returns(Task.CompletedTask);
        var http = new DefaultHttpContext { RequestServices = new ServiceCollection().AddSingleton(_authentication.Object).BuildServiceProvider() };
        var url = new Mock<IUrlHelper>();
        url.Setup(u => u.IsLocalUrl(It.IsAny<string?>())).Returns<string?>(u => u is not null && u.StartsWith('/') && !u.StartsWith("//"));
        return new LoginModel(_auth.Object, _audit.Object)
        {
            PageContext = new PageContext { HttpContext = http },
            Url = url.Object,
            Email = "ada@omni.test",
            Password = "correct-horse-1",
            ReturnUrl = returnUrl,
        };
    }

    [Fact]
    public async Task Success_issues_the_portal_cookie_audits_and_goes_back_where_the_admin_was()
    {
        _auth.Setup(a => a.SignInAsync("ada@omni.test", "correct-horse-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AdminSignInResult(TestKit.Ada, null));

        var result = await Page("/Clients").OnPostAsync(default);

        Assert.Equal("/Clients", Assert.IsType<LocalRedirectResult>(result).Url);
        Assert.NotNull(_signedIn);
        Assert.Equal(TestKit.Ada.Id, _signedIn.FindFirstValue(ClaimTypes.NameIdentifier));
        Assert.Equal("Ada Admin", _signedIn.FindFirstValue(ClaimTypes.Name));
        _audit.Verify(a => a.LogAsync("admin.signed_in", "admin_user", TestKit.Ada.Id, null, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task An_outside_return_url_goes_home()
    {
        _auth.Setup(a => a.SignInAsync(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AdminSignInResult(TestKit.Ada, null));

        var result = await Page("https://evil.test/").OnPostAsync(default);

        Assert.Equal("/", Assert.IsType<LocalRedirectResult>(result).Url);
    }

    [Theory]
    [InlineData(AdminAuthService.SignInFailed)]
    [InlineData(AdminAuthService.SignInUnavailable)]
    public async Task A_refused_or_unavailable_sign_in_stays_on_the_page_with_the_message(string message)
    {
        _auth.Setup(a => a.SignInAsync(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AdminSignInResult(null, message));
        var page = Page();

        var result = await page.OnPostAsync(default);

        Assert.IsType<PageResult>(result);
        Assert.Equal(message, page.Error);
        Assert.Null(_signedIn);
        _audit.VerifyNoOtherCalls();
    }
}
