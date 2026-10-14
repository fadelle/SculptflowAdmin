namespace SculptFlowAdmin.Entities.Requests.KnowledgeBenchmark;

/// <summary>Copy of the main app's body for POST knowledge-benchmark/clinics/{clinicId}/runs. Scope = all | generated |
/// reviewed | generation; GenerationId is required for "generation" (and only then).</summary>
public record StartBenchmarkRunRequest(string? Scope, Guid? GenerationId = null);
