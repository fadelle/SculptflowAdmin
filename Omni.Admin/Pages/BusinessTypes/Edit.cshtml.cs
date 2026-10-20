using Microsoft.AspNetCore.Mvc;
using Omni.Admin.Business.Contracts.Services.BusinessTypes;
using Omni.Admin.Business.Mappers.BusinessTypes;
using Omni.Admin.Common.Exceptions;
using Omni.Admin.Common.Helpers;
using Omni.Admin.Entities.Dtos.BusinessTypes;
using Omni.Admin.Pages.Shared;

namespace Omni.Admin.Pages.BusinessTypes;

/// <summary>Create (no code) or edit (code in the query) one business type; <c>?copy=code</c> starts a new one from an existing one.</summary>
public class EditModel : AdminPageModel
{
    private readonly IBusinessTypeAdminService _types;

    public EditModel(IBusinessTypeAdminService types) => _types = types;

    [BindProperty(SupportsGet = true)] public string? Code { get; set; }
    [BindProperty(SupportsGet = true)] public string? Copy { get; set; }
    [BindProperty] public BusinessTypeForm Form { get; set; } = new();
    public bool IsNew => string.IsNullOrWhiteSpace(Code);
    public int ClientCount { get; private set; }
    public bool IsActive { get; private set; }
    /// <summary>The saved type's missing Arabic / French text (empty for a new type).</summary>
    public IReadOnlyList<TranslationGap> Gaps { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        if (IsNew)
        {
            var source = !string.IsNullOrWhiteSpace(Copy) ? await _types.GetAsync(Copy, ct) : (await _types.ListAsync(ct)).FirstOrDefault();
            Form = source is null ? new BusinessTypeForm() : BusinessTypeFormMapper.FromResponse(source, forNewCopy: true);
            BusinessTypeFormMapper.WithSpareRows(Form);
            return Page();
        }
        var type = await _types.GetAsync(Code!, ct);
        if (type is null) return NotFound();
        ClientCount = type.ClientCount;
        IsActive = type.IsActive;
        Gaps = TranslationGaps.Of(type);
        Form = BusinessTypeFormMapper.WithSpareRows(BusinessTypeFormMapper.FromResponse(type));
        return Page();
    }

    /// <summary>Saves; on a refusal the form stays as the admin typed it, with the reason shown.</summary>
    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        try
        {
            var request = BusinessTypeFormMapper.ToRequest(Form);
            if (IsNew)
            {
                var created = await _types.CreateAsync(request, ct);
                Flash = "Business type created.";
                return RedirectToPage(null, new { code = created.Code });
            }
            await _types.UpdateAsync(Code!, request, ct);
            Flash = "Business type saved.";
            return RedirectToPage(null, new { code = Code });
        }
        catch (Exception ex) when (ex is AdminRuleException or ArgumentException)
        {
            FlashError = ex.Message;
        }
        catch (KeyNotFoundException)
        {
            FlashError = "That business type no longer exists.";
        }
        if (!IsNew)
        {
            var type = await _types.GetAsync(Code!, ct);
            ClientCount = type?.ClientCount ?? 0;
            IsActive = type?.IsActive ?? false;
            Gaps = type is null ? [] : TranslationGaps.Of(type);
        }
        return Page();
    }
}
