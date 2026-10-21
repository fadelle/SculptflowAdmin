using Moq;
using Omni.Admin.Business.Contracts.HttpClients.MainApp;
using Omni.Admin.Business.Contracts.Managers;
using Omni.Admin.Business.Services.Clients;
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
}
