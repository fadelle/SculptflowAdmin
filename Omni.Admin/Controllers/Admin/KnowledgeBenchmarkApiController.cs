using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Omni.Admin.Business.Contracts.Services.KnowledgeBenchmark;
using Omni.Admin.Common.Exceptions;
using Omni.Admin.Entities.Requests.KnowledgeBenchmark;

namespace Omni.Admin.Controllers.Admin;

/// <summary>
/// What the Search benchmark pages' scripts call (wwwroot/js/knowledge-benchmark*.js): /api/admin/knowledge-benchmark/{clientId}/…,
/// the same routes as the main app's /api/platform-admin/knowledge-benchmark/clients/{clientId}/…. Signed-in admins only;
/// every change is audited by the service. The main app's refusals come back with its status and { error } (409 a run or
/// generation is already in progress, 422 the generation already finished, 503 the generator isn't configured…), and a
/// main app that can't be reached is 502.
/// </summary>
[ApiController]
[Authorize]
[Route("api/admin/knowledge-benchmark/{clientId:guid}")]
public class KnowledgeBenchmarkApiController : ControllerBase
{
    private readonly IKnowledgeBenchmarkAdminService _benchmark;

    public KnowledgeBenchmarkApiController(IKnowledgeBenchmarkAdminService benchmark) => _benchmark = benchmark;

    [HttpGet]
    public Task<IActionResult> Dashboard(Guid clientId, CancellationToken ct) => Read(() => _benchmark.DashboardAsync(clientId, ct));

    [HttpGet("cases")]
    public Task<IActionResult> Cases(Guid clientId, string? view, Guid? generationId, int skip = 0, int take = 50, CancellationToken ct = default) =>
        Read(() => _benchmark.ListCasesAsync(clientId, view, generationId, skip, take, ct));

    [HttpGet("cases/{id:guid}")]
    public Task<IActionResult> Case(Guid clientId, Guid id, CancellationToken ct) => Read(() => _benchmark.GetCaseAsync(clientId, id, ct));

    [HttpPost("cases")]
    public Task<IActionResult> CreateCase(Guid clientId, CreateManualBenchmarkCaseRequest body, CancellationToken ct) =>
        Write(() => _benchmark.CreateManualCaseAsync(clientId, body, ct));

    [HttpPut("cases/{id:guid}")]
    public Task<IActionResult> UpdateCase(Guid clientId, Guid id, UpdateBenchmarkCaseRequest body, CancellationToken ct) =>
        Write(() => _benchmark.UpdateCaseAsync(clientId, id, body, ct));

    [HttpPost("cases/{id:guid}/review")]
    public Task<IActionResult> Review(Guid clientId, Guid id, SetBenchmarkCaseReviewedRequest body, CancellationToken ct) =>
        Write(() => _benchmark.SetReviewedAsync(clientId, id, body, ct));

    [HttpDelete("cases/{id:guid}")]
    public Task<IActionResult> DeleteCase(Guid clientId, Guid id, CancellationToken ct) =>
        Write(async () =>
        {
            await _benchmark.DeleteCaseAsync(clientId, id, ct);
            return NoContent();
        });

    [HttpPost("cases/generate")]
    public Task<IActionResult> Generate(Guid clientId, CancellationToken ct) => Write(() => _benchmark.GenerateCasesAsync(clientId, ct));

    [HttpGet("source-documents")]
    public Task<IActionResult> SourceDocuments(Guid clientId, CancellationToken ct) => Read(() => _benchmark.SourceDocumentsAsync(clientId, ct));

    [HttpGet("source-documents/{documentId:guid}/chunks")]
    public Task<IActionResult> SourceChunks(Guid clientId, Guid documentId, CancellationToken ct) =>
        Read(() => _benchmark.SourceChunksAsync(clientId, documentId, ct));

    [HttpGet("generations")]
    public Task<IActionResult> Generations(Guid clientId, int take = 20, CancellationToken ct = default) =>
        Read(() => _benchmark.ListGenerationsAsync(clientId, take, ct));

    [HttpGet("generations/{id:guid}")]
    public Task<IActionResult> Generation(Guid clientId, Guid id, CancellationToken ct) => Read(() => _benchmark.GetGenerationAsync(clientId, id, ct));

    [HttpPost("generations/{id:guid}/cancel")]
    public Task<IActionResult> CancelGeneration(Guid clientId, Guid id, CancellationToken ct) =>
        Write(() => _benchmark.CancelGenerationAsync(clientId, id, ct));

    [HttpPost("runs")]
    public Task<IActionResult> StartRun(Guid clientId, StartBenchmarkRunRequest body, CancellationToken ct) =>
        Write(() => _benchmark.StartRunAsync(clientId, body, ct));

    [HttpGet("runs")]
    public Task<IActionResult> Runs(Guid clientId, int take = 20, CancellationToken ct = default) => Read(() => _benchmark.ListRunsAsync(clientId, take, ct));

    [HttpGet("runs/{id:guid}")]
    public Task<IActionResult> Run(Guid clientId, Guid id, CancellationToken ct) => Read(() => _benchmark.GetRunAsync(clientId, id, ct));

    [HttpGet("runs/{id:guid}/results")]
    public Task<IActionResult> Results(Guid clientId, Guid id, string? classification, int skip = 0, int take = 100, CancellationToken ct = default) =>
        Read(() => _benchmark.ListResultsAsync(clientId, id, classification, skip, take, ct));

    [HttpGet("runs/{id:guid}/generations")]
    public Task<IActionResult> RunGenerations(Guid clientId, Guid id, CancellationToken ct) => Read(() => _benchmark.RunGenerationsAsync(clientId, id, ct));

    [HttpGet("runs/{id:guid}/results/{resultId:guid}")]
    public Task<IActionResult> Result(Guid clientId, Guid id, Guid resultId, CancellationToken ct) =>
        Read(() => _benchmark.GetResultAsync(clientId, id, resultId, ct));

    // ------------------------------------------------------------------

    /// <summary>200 with the main app's answer, 404 when it has no such client or record.</summary>
    private async Task<IActionResult> Read(Func<Task<JsonNode?>> read)
    {
        try
        {
            return await read() is { } node ? Ok(node) : NotFound(new { error = "Not found. It may have been deleted, or it isn't this client's." });
        }
        catch (MainAppApiException ex) { return Refused(ex); }
    }

    /// <summary>200 with the changed record, 404 for a missing record, otherwise the main app's own status and message so
    /// the page can show why.</summary>
    private Task<IActionResult> Write(Func<Task<JsonNode>> write) => Write(async () => (IActionResult)Ok(await write()));

    private async Task<IActionResult> Write(Func<Task<IActionResult>> write)
    {
        try
        {
            return await write();
        }
        catch (KeyNotFoundException) { return NotFound(new { error = "Not found. It may have been deleted, or it isn't this client's." }); }
        catch (MainAppApiException ex) { return Refused(ex); }
    }

    // The main app's own 4xx/5xx is passed on as is; no status at all means it couldn't be reached (502).
    private ObjectResult Refused(MainAppApiException ex) =>
        StatusCode(ex.Status is { } status ? (int)status : StatusCodes.Status502BadGateway, new { error = ex.Message });
}
