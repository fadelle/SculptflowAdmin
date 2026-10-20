using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using Omni.Admin.Business.Contracts.HttpClients.MainApp;
using Omni.Admin.Common.Configs;

namespace Omni.Admin.Business.HttpClients.MainApp;

/// <summary>Client for the main app's platform-admin Knowledge Retrieval Benchmark API
/// (/api/platform-admin/knowledge-benchmark/clients/{clientId}).</summary>
public class KnowledgeBenchmarkApiClient : MainAppApiClient, IKnowledgeBenchmarkApiClient
{
    public const string Prefix = "api/platform-admin/knowledge-benchmark/clients/";

    public KnowledgeBenchmarkApiClient(HttpClient http, IOptions<MainAppApiOptions> options, IHttpContextAccessor context,
        ILogger<KnowledgeBenchmarkApiClient> logger)
        : base(http, options, context, logger, Prefix, "Search benchmark")
    {
    }

    public Task<JsonNode?> GetAsync(Guid clientId, string path, CancellationToken ct) => GetAsync<JsonNode>(clientId + path, ct);

    public Task<JsonNode> SendAsync(Guid clientId, HttpMethod method, string path, object? body, CancellationToken ct) =>
        WriteForAsync<JsonNode>(method, clientId + path, body, null, false, ct);

    public Task DeleteAsync(Guid clientId, string path, CancellationToken ct) => WriteAsync(HttpMethod.Delete, clientId + path, null, null, false, ct);
}
