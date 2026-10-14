using Microsoft.AspNetCore.Mvc;
using SculptFlowAdmin.Business.Contracts.Services.Channels;
using SculptFlowAdmin.Common.Exceptions;
using SculptFlowAdmin.Entities.Responses.PlatformAdmin;
using SculptFlowAdmin.Pages.Shared;

namespace SculptFlowAdmin.Pages.Channels;

/// <summary>Diagnostic for the embedded-signup setup: what permissions Meta granted a token and which client WhatsApp
/// business accounts it can see. The answer is shown inline on the POST response; the token never goes into a URL,
/// TempData or the form again.</summary>
public class TokenDebugModel : MainAppPageModel
{
    private readonly IChannelAdminService _channels;

    public TokenDebugModel(IChannelAdminService channels) => _channels = channels;

    public WhatsAppTokenDebugResult? Result { get; private set; }
    public string? Error { get; private set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(string? accessToken, string? code, string? redirectUri, CancellationToken ct)
    {
        try
        {
            Result = await _channels.DebugWhatsAppTokenAsync(accessToken, code, redirectUri, ct);
        }
        catch (Exception ex) when (ex is AdminRuleException or ArgumentException)
        {
            Error = ex.Message;
        }
        return Page();
    }
}