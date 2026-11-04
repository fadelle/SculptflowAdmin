using Omni.Admin.Entities.Responses.PlatformAdmin;
using Omni.Admin.Pages.Clients;

namespace Omni.Admin.Tests.Pages;

/// <summary>QA t6-admin-20: the "New client" page tells the admin what happened to the plan and the owner invite.</summary>
public class NewClientModelTests
{
    private static ClientCreated Created(string? plan, bool planStarted, bool inviteSent) =>
        new(Guid.NewGuid(), "glow", plan, planStarted, inviteSent, inviteSent ? DateTimeOffset.UtcNow.AddDays(7) : null);

    [Fact]
    public void The_outcome_names_the_plan_and_where_the_invite_went()
    {
        Assert.Equal("Client opened (glow), plan growth. The owner invite went to olga@x.com.", NewModel.Outcome(Created("growth", true, true), "olga@x.com"));
        Assert.Equal("Client opened (glow), no plan yet: growth didn't start (assign it on its Billing page). The owner invite wasn't sent.",
            NewModel.Outcome(Created("growth", false, false), "olga@x.com"));
        Assert.StartsWith("Client opened (glow), no plan (assign one", NewModel.Outcome(Created(null, false, true), "olga@x.com"));
    }
}
