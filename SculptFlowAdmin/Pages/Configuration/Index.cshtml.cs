using Microsoft.AspNetCore.Mvc;
using SculptFlowAdmin.Business.Contracts.Services.Configuration;
using SculptFlowAdmin.Entities.Responses.Configuration;
using SculptFlowAdmin.Pages.Shared;

namespace SculptFlowAdmin.Pages.Configuration;

/// <summary>The main app's settings (config.settings by section and key, else a constant default), through its settings API.</summary>
public class IndexModel : MainAppPageModel
{
    private readonly ISettingsAdminService _settings;

    public IndexModel(ISettingsAdminService settings) => _settings = settings;

    public string? Q { get; set; }
    public bool ChangedOnly { get; set; }
    public List<SettingResponse> Settings { get; private set; } = new();
    public List<IGrouping<string, SettingResponse>> Sections { get; private set; } = new();

    public async Task OnGetAsync(string? q, bool changed, CancellationToken ct)
    {
        Q = Clean(q);
        ChangedOnly = changed;
        await LoadAsync(async () => Settings = await _settings.ListAsync(ct));
        Sections = Settings
            .Where(s => Q is null || $"{s.Section}:{s.Key}".Contains(Q, StringComparison.OrdinalIgnoreCase)
                                  || s.Description.Contains(Q, StringComparison.OrdinalIgnoreCase))
            .Where(s => !ChangedOnly || s.StoredValue is not null)
            .GroupBy(s => s.Section)
            .ToList();
    }

    public Task<IActionResult> OnPostSetAsync(string? section, string? key, string? value, string? note, bool isSecret, CancellationToken ct) =>
        RunAsync(() => _settings.SetAsync(section ?? "", key ?? "", value, note, isSecret, ct), $"{section}:{key} saved. The main app uses it now.");

    public Task<IActionResult> OnPostResetAsync(string? section, string? key, CancellationToken ct) =>
        RunAsync(() => _settings.ResetAsync(section ?? "", key ?? "", ct), $"{section}:{key} is back to its default.");
}
