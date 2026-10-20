using Microsoft.Extensions.Options;
using Omni.Admin.Business.Contracts.HttpClients.MainApp;
using Omni.Admin.Common.Configs;
using Omni.Admin.Entities.Requests.BusinessTypes;
using Omni.Admin.Entities.Requests.Content;
using Omni.Admin.Entities.Responses.BusinessTypes;

namespace Omni.Admin.Business.HttpClients.MainApp;

/// <summary>Client for the main app's platform-admin business types API (/api/platform-admin/business-types).</summary>
public class BusinessTypesApiClient : MainAppApiClient, IBusinessTypesApiClient
{
    public const string Prefix = "api/platform-admin/business-types";

    public BusinessTypesApiClient(HttpClient http, IOptions<MainAppApiOptions> options, IHttpContextAccessor context,
        ILogger<BusinessTypesApiClient> logger)
        : base(http, options, context, logger, Prefix, "Business types")
    {
    }

    public Task<List<BusinessTypeResponse>> ListAsync(CancellationToken ct) => GetRequiredAsync<List<BusinessTypeResponse>>("", ct);

    public Task<BusinessTypeResponse?> GetAsync(string code, CancellationToken ct) => GetAsync<BusinessTypeResponse>($"/{Esc(code)}", ct);

    public Task<BusinessTypeResponse> CreateAsync(SaveBusinessTypeRequest request, CancellationToken ct) =>
        WriteForAsync<BusinessTypeResponse>(HttpMethod.Post, "", request, null, true, ct);

    public Task<BusinessTypeResponse> UpdateAsync(string code, SaveBusinessTypeRequest request, CancellationToken ct) =>
        WriteForAsync<BusinessTypeResponse>(HttpMethod.Put, $"/{Esc(code)}", request, null, false, ct);

    public Task<BusinessTypeResponse> SetActiveAsync(string code, bool active, CancellationToken ct) =>
        WriteForAsync<BusinessTypeResponse>(HttpMethod.Put, $"/{Esc(code)}/active", new ActiveBody(active), null, false, ct);
}
