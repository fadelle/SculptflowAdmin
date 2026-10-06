using Microsoft.AspNetCore.Mvc;
using SculptFlowAdmin.Business.Contracts.Services.Staff;
using SculptFlowAdmin.Entities.Dtos.Staff;
using SculptFlowAdmin.Pages.Shared;

namespace SculptFlowAdmin.Pages.Staff;

public class DetailsModel : AdminPageModel
{
    private readonly IStaffAdminService _staff;

    public DetailsModel(IStaffAdminService staff) => _staff = staff;

    [BindProperty(SupportsGet = true)] public string Id { get; set; } = string.Empty;
    public StaffRow Staff { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var s = await _staff.GetAsync(Id, ct);
        if (s is null) return NotFound();
        Staff = s;
        return Page();
    }

    public Task<IActionResult> OnPostMembershipAsync(bool active, CancellationToken ct) =>
        RunAsync(() => _staff.SetMembershipActiveAsync(Id, active, ct),
            active ? "Clinic membership activated." : "Clinic membership deactivated.", new { id = Id });

    public Task<IActionResult> OnPostLockAsync(bool locked, CancellationToken ct) =>
        RunAsync(() => _staff.SetLoginLockedAsync(Id, locked, ct),
            locked ? "Sign-in locked." : "Sign-in unlocked.", new { id = Id });

    public Task<IActionResult> OnPostPasswordAsync(string password, CancellationToken ct) =>
        RunAsync(() => _staff.SetPasswordAsync(Id, password, ct), "Password changed. Tell the user their new password.", new { id = Id });

    public Task<IActionResult> OnPostEmailConfirmedAsync(bool confirmed, CancellationToken ct) =>
        RunAsync(() => _staff.SetEmailConfirmedAsync(Id, confirmed, ct), "Email confirmation updated.", new { id = Id });
}
