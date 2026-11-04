using Microsoft.AspNetCore.Mvc;
using Omni.Admin.Business.Contracts.Services.Billing;
using Omni.Admin.Business.Contracts.Services.BusinessTypes;
using Omni.Admin.Business.Contracts.Services.Clients;
using Omni.Admin.Common.Exceptions;
using Omni.Admin.Entities.Requests.Clients;
using Omni.Admin.Entities.Responses.Billing;
using Omni.Admin.Entities.Responses.BusinessTypes;
using Omni.Admin.Entities.Responses.PlatformAdmin;
using Omni.Admin.Pages.Shared;

namespace Omni.Admin.Pages.Clients;

/// <summary>Open a client for someone the team sold to (QA t6-admin-20): name, business type, plan, dashboard language and the
/// owner, who gets an emailed link to set their password. The main app checks every field; a refusal shows the page again with
/// what was typed.</summary>
public class NewModel : AdminPageModel
{
    private readonly IClientAdminService _clients;
    private readonly IBusinessTypeAdminService _types;
    private readonly IBillingAdminService _billing;

    public NewModel(IClientAdminService clients, IBusinessTypeAdminService types, IBillingAdminService billing)
    {
        _clients = clients;
        _types = types;
        _billing = billing;
    }

    [BindProperty] public string? Name { get; set; }
    [BindProperty] public string? BusinessTypeCode { get; set; }
    [BindProperty] public string? OwnerEmail { get; set; }
    [BindProperty] public string? OwnerName { get; set; }
    [BindProperty] public string? PlanCode { get; set; }
    [BindProperty] public string Language { get; set; } = "en";

    public List<BusinessTypeResponse> Types { get; private set; } = new();
    public List<PlanResponse> Plans { get; private set; } = new();
    public string? Error { get; private set; }

    public async Task OnGetAsync(CancellationToken ct) => await LoadAsync(ct);

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        try
        {
            var created = await _clients.CreateAsync(new CreateClientRequest(Name, BusinessTypeCode, OwnerEmail, OwnerName, PlanCode, Language), ct);
            Flash = Outcome(created, OwnerEmail!.Trim());
            if (!created.InviteSent) FlashError = "The invite email didn't go. Use \"Send the owner invite again\" below.";
            return RedirectToPage("/Clients/Details", new { id = created.ClientId });
        }
        catch (Exception ex) when (ex is AdminRuleException or ArgumentException)
        {
            Error = ex.Message;
        }
        await LoadAsync(ct);
        return Page();
    }

    /// <summary>The success line: the client, its plan, and where the invite went.</summary>
    public static string Outcome(ClientCreated created, string ownerEmail)
    {
        var plan = created.PlanCode is null ? "no plan (assign one on its Billing page)"
            : created.PlanStarted ? $"plan {created.PlanCode}" : $"no plan yet: {created.PlanCode} didn't start (assign it on its Billing page)";
        var invite = created.InviteSent ? $"The owner invite went to {ownerEmail}." : "The owner invite wasn't sent.";
        return $"Client opened ({created.Slug}), {plan}. {invite}";
    }

    private async Task LoadAsync(CancellationToken ct)
    {
        Types = (await _types.ListAsync(ct)).Where(t => t.IsActive).ToList();
        Plans = (await _billing.PlansAsync(ct)).Where(p => p.IsActive).OrderBy(p => p.SortOrder).ToList();
    }
}
