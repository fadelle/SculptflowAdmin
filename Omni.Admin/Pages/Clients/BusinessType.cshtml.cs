using Microsoft.AspNetCore.Mvc;
using Omni.Admin.Business.Contracts.Services.BusinessTypes;
using Omni.Admin.Business.Contracts.Services.Clients;
using Omni.Admin.Common.Exceptions;
using Omni.Admin.Entities.Dtos.BusinessTypes;
using Omni.Admin.Entities.Responses.BusinessTypes;
using Omni.Admin.Entities.Responses.PlatformAdmin;
using Omni.Admin.Pages.Shared;

namespace Omni.Admin.Pages.Clients;

/// <summary>Change a client's business type in two steps: pick the type, see how many contacts move to which stage, then confirm.</summary>
public class BusinessTypeModel : AdminPageModel
{
    private readonly IClientAdminService _clients;
    private readonly IBusinessTypeAdminService _types;

    public BusinessTypeModel(IClientAdminService clients, IBusinessTypeAdminService types)
    {
        _clients = clients;
        _types = types;
    }

    [BindProperty(SupportsGet = true)] public Guid Id { get; set; }
    [BindProperty(SupportsGet = true)] public string? To { get; set; }
    public ClientDetail Client { get; private set; } = null!;
    public List<BusinessTypeResponse> Types { get; private set; } = new();
    public BusinessTypeChangePreview? Preview { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        if (!await LoadAsync(ct)) return NotFound();
        if (!string.IsNullOrWhiteSpace(To) && To != Client.BusinessTypeCode) await TryPreviewAsync(ct);
        return Page();
    }

    public async Task<IActionResult> OnPostConfirmAsync(CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(To)) throw new AdminRuleException("Pick the business type to move this client to.");
            await _clients.ChangeBusinessTypeAsync(Id, To, ct);
            Flash = "Business type changed. The client's contacts were moved to the matching stages.";
            return RedirectToPage("/Clients/Details", new { id = Id });
        }
        catch (Exception ex) when (ex is AdminRuleException or ArgumentException)
        {
            FlashError = ex.Message;
        }
        catch (KeyNotFoundException)
        {
            FlashError = "That client no longer exists.";
        }
        if (!await LoadAsync(ct)) return NotFound();
        return Page();
    }

    private async Task<bool> LoadAsync(CancellationToken ct)
    {
        var client = await _clients.GetAsync(Id, ct);
        if (client is null) return false;
        Client = client;
        Types = (await _types.ListAsync(ct)).Where(t => t.IsActive || t.Code == client.BusinessTypeCode).ToList();
        return true;
    }

    private async Task TryPreviewAsync(CancellationToken ct)
    {
        try
        {
            Preview = await _clients.PreviewBusinessTypeAsync(Id, To!, ct);
        }
        catch (AdminRuleException ex)
        {
            FlashError = ex.Message;
        }
    }
}
