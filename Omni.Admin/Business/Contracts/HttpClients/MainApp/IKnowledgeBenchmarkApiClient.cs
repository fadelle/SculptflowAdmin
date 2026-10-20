using System.Text.Json.Nodes;

namespace Omni.Admin.Business.Contracts.HttpClients.MainApp;

/// <summary>
/// The main app's platform-admin benchmark API (/api/platform-admin/knowledge-benchmark/clients/{clientId}). Answers are
/// handed through as JSON for the benchmark page's script (wwwroot/js/knowledge-benchmark*.js), which is the only reader;
/// the request bodies are typed copies. <paramref name="path"/> is relative to the client's root ("" for the dashboard,
/// "/cases?view=manual", "/runs/{id}") and includes its query string.
/// </summary>
public interface IKnowledgeBenchmarkApiClient
{
    /// <summary>Null when the main app answers 404 (unknown client or record).</summary>
    Task<JsonNode?> GetAsync(Guid clientId, string path, CancellationToken ct);

    /// <summary>A POST/PUT whose answer the page needs. 404 → KeyNotFoundException; other refusals → MainAppApiException
    /// with the main app's status and message.</summary>
    Task<JsonNode> SendAsync(Guid clientId, HttpMethod method, string path, object? body, CancellationToken ct);

    /// <summary>A DELETE (no answer body). 404 → KeyNotFoundException.</summary>
    Task DeleteAsync(Guid clientId, string path, CancellationToken ct);
}
