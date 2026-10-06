using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using SculptFlowAdmin.Common.Exceptions;

namespace SculptFlowAdmin.Pages.Shared;

/// <summary>Base for billing pages: a GET that fails to reach the main app shows the reason instead of an error page.</summary>
public abstract class BillingPageModel : AdminPageModel
{
    public string? LoadError { get; protected set; }

    /// <summary>A fresh operation id for money-moving forms; it becomes the API's Idempotency-Key on submit, so a
    /// double-clicked or re-posted form applies once.</summary>
    public string NewOperationId() => Guid.NewGuid().ToString("N");

    protected async Task<bool> LoadAsync(Func<Task> load)
    {
        try
        {
            await load();
            return true;
        }
        catch (AdminRuleException ex)
        {
            LoadError = ex.Message;
            return false;
        }
    }

    protected static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
