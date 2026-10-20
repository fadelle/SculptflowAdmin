using Microsoft.AspNetCore.Mvc;
using Omni.Admin.Business.Contracts.Services.Members;
using Omni.Admin.Entities.Dtos.Members;
using Omni.Admin.Pages.Shared;

namespace Omni.Admin.Pages.Members;

public class DetailsModel : AdminPageModel
{
    private readonly IMemberAdminService _members;

    public DetailsModel(IMemberAdminService member) => _members = member;

    [BindProperty(SupportsGet = true)] public string Id { get; set; } = string.Empty;
    public MemberRow Member { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var s = await _members.GetAsync(Id, ct);
        if (s is null) return NotFound();
        Member = s;
        return Page();
    }

    public Task<IActionResult> OnPostMembershipAsync(bool active, CancellationToken ct) =>
        RunAsync(() => _members.SetMembershipActiveAsync(Id, active, ct),
            active ? "Client membership activated." : "Client membership deactivated.", new { id = Id });

    public Task<IActionResult> OnPostLockAsync(bool locked, CancellationToken ct) =>
        RunAsync(() => _members.SetLoginLockedAsync(Id, locked, ct),
            locked ? "Sign-in locked." : "Sign-in unlocked.", new { id = Id });

    public Task<IActionResult> OnPostPasswordAsync(string password, CancellationToken ct) =>
        RunAsync(() => _members.SetPasswordAsync(Id, password, ct), "Password changed. Tell the user their new password.", new { id = Id });

    public Task<IActionResult> OnPostEmailConfirmedAsync(bool confirmed, CancellationToken ct) =>
        RunAsync(() => _members.SetEmailConfirmedAsync(Id, confirmed, ct), "Email confirmation updated.", new { id = Id });
}
