using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SculptFlowAdmin.Business.Contracts.Managers;
using SculptFlowAdmin.Common.Exceptions;

namespace SculptFlowAdmin.Controllers.Admin;

/// <summary>Choices for the header's clinic scope selector (aurora.js): GET /api/admin/scope/options?q=&amp;skip=&amp;take=.
/// Signed-in admins only, like the rest of /api/admin.</summary>
[ApiController]
[Authorize]
[Route("api/admin/scope")]
public class ScopeApiController : ControllerBase
{
    private readonly IScopeLookup _lookup;

    public ScopeApiController(IScopeLookup lookup) => _lookup = lookup;

    [HttpGet("options")]
    public async Task<IActionResult> Options(string? q, int? skip, int? take, CancellationToken ct)
    {
        try
        {
            return Ok(await _lookup.SearchAsync(q, Math.Max(0, skip ?? 0), Math.Clamp(take ?? 25, 1, 50), ct));
        }
        catch (MainAppApiException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { error = ex.Message });
        }
    }
}
