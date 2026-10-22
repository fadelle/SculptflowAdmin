using Moq;
using Omni.Admin.Business.Contracts.HttpClients.MainApp;
using Omni.Admin.Business.Contracts.Managers;
using Omni.Admin.Business.Services.Billing;
using Omni.Admin.Entities.Dtos.Billing;
using Omni.Admin.Entities.Requests.Billing;

namespace Omni.Admin.Tests.Services;

/// <summary>Milestone C P2: add-on writes go through the main app and are audited.</summary>
public class BillingAdminServiceTests
{
    private readonly Mock<IBillingApiClient> _api = new();
    private readonly Mock<IAdminAudit> _audit = new();
    private readonly Guid _clientId = Guid.NewGuid();

    private BillingAdminService Sut() => new(_api.Object, _audit.Object);

    [Fact]
    public async Task Setting_a_clients_add_on_units_is_sent_with_the_forms_operation_id_and_audited()
    {
        _api.Setup(a => a.SetClientAddonAsync(_clientId, "extra-ai-bot", It.IsAny<SetClientAddonRequest>(), "op-9", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ClientAddonRow>());

        await Sut().SetClientAddonAsync(_clientId, " extra-ai-bot ", 2, "op-9", default);

        _api.Verify(a => a.SetClientAddonAsync(_clientId, "extra-ai-bot", new SetClientAddonRequest(2), "op-9", It.IsAny<CancellationToken>()), Times.Once);
        _audit.Verify(a => a.LogAsync("billing.client_addon_set", "billing_client_addon", "extra-ai-bot", _clientId, It.IsAny<object?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("", 1)]
    [InlineData("extra-user", -1)]
    public async Task A_missing_add_on_or_negative_units_never_reach_the_main_app(string code, int quantity)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Sut().SetClientAddonAsync(_clientId, code, quantity, "op", default));
        _api.VerifyNoOtherCalls();
        _audit.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Saving_a_catalog_add_on_is_audited_under_its_code()
    {
        var request = new AddonRequest("Extra user", "max_users", 8m);
        await Sut().SaveAddonAsync("extra-user", request, default);
        _api.Verify(a => a.SaveAddonAsync("extra-user", request, It.IsAny<CancellationToken>()), Times.Once);
        _audit.Verify(a => a.LogAsync("billing.addon_save", "billing_addon", "extra-user", null, request, It.IsAny<CancellationToken>()), Times.Once);
    }
}
