using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using SculptFlowAdmin.Business.Contracts.HttpClients.MainApp;
using SculptFlowAdmin.Common.Configs;

namespace SculptFlowAdmin.Business.HttpClients.MainApp;

/// <summary>Client for the main app's platform-admin Knowledge Retrieval Benchmark API
/// (/api/platform-admin/knowledge-benchmark/clinics/{clinicId}).</summary>
public class KnowledgeBenchmarkApiClient : MainAppApiClient, IKnowledgeBenchmarkApiClient
{
    public const string Prefix = "api/platform-admin/knowledge-benchmark/clinics/";

    public KnowledgeBenchmarkApiClient(HttpClient http, IOptions<MainAppApiOptions> options, IHttpContextAccessor context,
        ILogger<KnowledgeBenchmarkApiClient> logger)
        : base(http, options, context, logger, Prefix, "Search benchmark")
    {
    }

    public Task<JsonNode?> GetAsync(Guid clinicId, string path, CancellationToken ct) => GetAsync<JsonNode>(clinicId + path, ct);

    public Task<JsonNode> SendAsync(Guid clinicId, HttpMethod method, string path, object? body, CancellationToken ct) =>
        WriteForAsync<JsonNode>(method, clinicId + path, body, null, false, ct);

    public Task DeleteAsync(Guid clinicId, string path, CancellationToken ct) => WriteAsync(HttpMethod.Delete, clinicId + path, null, null, false, ct);
}
