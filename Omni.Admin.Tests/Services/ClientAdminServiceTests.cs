using Moq;
using Omni.Admin.Business.Contracts.HttpClients.MainApp;
using Omni.Admin.Business.Contracts.Managers;
using Omni.Admin.Business.Services.Clients;
using Omni.Admin.Common.Exceptions;
using Omni.Admin.Entities.Requests.Clients;
using Omni.Admin.Entities.Responses.PlatformAdmin;

namespace Omni.Admin.Tests.Services;

public class ClientAdminServiceTests
{
    private static readonly Guid Id = Guid.Parse("c1000000-0000-4000-8000-000000000001");

    private static readonly ClientDetail English = new(Id, "Demo", "demo", null, null, null, null, "UTC", null, null, null, true,
        DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, "general", "en");

    private readonly Mock<IClientsApiClient> _api = new(MockBehavior.Strict);
    private readonly Mock<IAdminAudit> _audit = new();

    private ClientAdminService Service() => new(_api.Object, _audit.Object);

    [Fact]
    public async Task SetDefaultLanguage_changes_it_in_the_main_app_and_audits_from_and_to()
    {
        _api.Setup(a => a.GetAsync(Id, It.IsAny<CancellationToken>())).ReturnsAsync(English);
        _api.Setup(a => a.SetDefaultLanguageAsync(Id, "ar", It.IsAny<CancellationToken>())).ReturnsAsync(new PlatformAdminChange(Id));
        object? details = null;
        _audit.Setup(a => a.LogAsync("client.default_language_changed", "client", Id, Id, It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .Callback<string, string, object?, Guid?, object?, CancellationToken>((_, _, _, _, d, _) => details = d)
            .Returns(Task.CompletedTask);

        await Service().SetDefaultLanguageAsync(Id, " AR ");

        _api.Verify(a => a.SetDefaultLanguageAsync(Id, "ar", It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal("{ from = en, to = ar }", details?.ToString());
    }

    [Fact]
    public async Task SetDefaultLanguage_to_the_same_language_calls_nothing_and_audits_nothing()
    {
        _api.Setup(a => a.GetAsync(Id, It.IsAny<CancellationToken>())).ReturnsAsync(English);

        await Service().SetDefaultLanguageAsync(Id, "en");

        _audit.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("")]
    [InlineData("es")]
    [InlineData("Arabic")]
    public async Task SetDefaultLanguage_refuses_a_language_the_dashboard_does_not_offer(string language)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Service().SetDefaultLanguageAsync(Id, language));

        _audit.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SetDefaultLanguage_on_a_client_that_no_longer_exists_is_not_found()
    {
        _api.Setup(a => a.GetAsync(Id, It.IsAny<CancellationToken>())).ReturnsAsync((ClientDetail?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => Service().SetDefaultLanguageAsync(Id, "fr"));
    }

    [Fact]
    public async Task Create_opens_the_client_in_the_main_app_and_audits_what_was_asked_and_what_happened()
    {
        // QA t6-admin-20.
        var request = new CreateClientRequest(" Glow ", "clinic", " olga@x.com ", "Olga", null, "ar");
        _api.Setup(a => a.CreateAsync(request, It.IsAny<CancellationToken>())).ReturnsAsync(new ClientCreated(Id, "glow", "starter", true, true, DateTimeOffset.UtcNow));
        object? details = null;
        _audit.Setup(a => a.LogAsync("client.created", "client", Id, Id, It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .Callback<string, string, object?, Guid?, object?, CancellationToken>((_, _, _, _, d, _) => details = d)
            .Returns(Task.CompletedTask);

        var created = await Service().CreateAsync(request);

        Assert.Equal("glow", created.Slug);
        Assert.Equal("{ name = Glow, businessType = clinic, ownerEmail = olga@x.com, language = ar, plan = starter, PlanStarted = True, InviteSent = True }", details?.ToString());
    }

    [Theory]
    [InlineData(" ", "clinic", "o@x.com")]
    [InlineData("Glow", "", "o@x.com")]
    [InlineData("Glow", "clinic", " ")]
    public async Task Create_needs_a_name_a_business_type_and_the_owners_email_before_calling_the_main_app(string name, string type, string email)
    {
        await Assert.ThrowsAsync<AdminRuleException>(() => Service().CreateAsync(new CreateClientRequest(name, type, email, null, null, "en")));
        _audit.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ResendOwnerInvite_is_audited_under_the_client()
    {
        _api.Setup(a => a.ResendOwnerInviteAsync(Id, It.IsAny<CancellationToken>())).ReturnsAsync(new OwnerInviteSent(Id, "olga@x.com", DateTimeOffset.UtcNow.AddDays(7)));

        await Service().ResendOwnerInviteAsync(Id);

        _audit.Verify(a => a.LogAsync("client.owner_invite_sent", "client", Id, Id, It.IsAny<object?>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
