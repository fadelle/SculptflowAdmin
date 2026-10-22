using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Omni.Admin.Business.HttpClients.MainApp;
using Omni.Admin.Entities.Requests.Billing;
using Omni.Admin.Tests.Support;

namespace Omni.Admin.Tests.HttpClients;

/// <summary>Milestone C P2: add-ons, trial plans and the client summary's new fields.</summary>
public class BillingApiClientTests
{
    private readonly FakeHttpHandler _http = new();

    private BillingApiClient Client() =>
        new(new HttpClient(_http), TestKit.Options(), TestKit.Accessor(TestKit.SignedIn(TestKit.Ada)), NullLogger<BillingApiClient>.Instance);

    [Fact]
    public async Task Add_ons_are_read_saved_and_set_per_client_with_an_idempotency_key()
    {
        var clientId = Guid.NewGuid();
        var op = Guid.NewGuid();
        _http.Enqueue(HttpStatusCode.OK, """[{"code":"extra-user","name":"Extra user","entitlementKey":"max_users","unitPrice":8.00,"currency":"USD","isActive":true,"sortOrder":10,"clients":2}]""");
        _http.Enqueue(HttpStatusCode.OK, """{"code":"extra-user"}""");
        _http.Enqueue(HttpStatusCode.OK, """[{"code":"extra-user","name":"Extra user","entitlementKey":"max_users","quantity":3,"unitPrice":8.00,"currency":"USD","periodPrice":24.00}]""");

        var catalog = Assert.Single(await Client().AddonsAsync(default));
        await Client().SaveAddonAsync("extra-user", new AddonRequest("Extra user", null, 9m), default);
        var rows = await Client().SetClientAddonAsync(clientId, "extra-user", new SetClientAddonRequest(3), op.ToString("N"), default);

        Assert.Equal(("max_users", 8m, 2), (catalog.EntitlementKey, catalog.UnitPrice, catalog.Clients));
        Assert.Equal((3, 24m), (Assert.Single(rows).Quantity, rows[0].PeriodPrice));
        Assert.Equal((HttpMethod.Get, "/api/platform-admin/billing/addons"), (_http.Requests[0].Method, _http.Requests[0].Uri.AbsolutePath));
        Assert.Equal((HttpMethod.Put, "/api/platform-admin/billing/addons/extra-user"), (_http.Requests[1].Method, _http.Requests[1].Uri.AbsolutePath));
        Assert.Contains("\"unitPrice\":9", _http.Requests[1].Body);
        Assert.Equal((HttpMethod.Put, $"/api/platform-admin/billing/clients/{clientId}/addons/extra-user"), (_http.Requests[2].Method, _http.Requests[2].Uri.AbsolutePath));
        Assert.Equal($"admin-portal:{op:N}", _http.Requests[2].Headers["Idempotency-Key"]);
        Assert.Contains("\"quantity\":3", _http.Requests[2].Body);
    }

    [Fact]
    public async Task A_plan_carries_its_trial_days_both_ways()
    {
        _http.Enqueue(HttpStatusCode.OK, """[{"id":"6f1c0000-0000-4000-8000-000000000001","code":"trial","name":"Free Trial","description":null,"price":0,"currency":"USD","billingPeriod":"month","includedUsageCredit":0,"rateCardCode":null,"isActive":true,"sortOrder":0,"entitlements":{"max_ai_bots":"0"},"subscribers":4,"trialDays":7}]""");
        _http.Enqueue(HttpStatusCode.OK, "{}");

        var plan = Assert.Single(await Client().PlansAsync(default));
        await Client().CreatePlanAsync(new PlanRequest("t14", "Trial 14", null, 0, "month", 0, null, TrialDays: 14), default);

        Assert.Equal(7, plan.TrialDays);
        Assert.Contains("\"trialDays\":14", _http.Requests[1].Body);
    }
}
