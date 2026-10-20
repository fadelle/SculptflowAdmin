using System.Net;
using System.Text;

namespace Omni.Admin.Tests.Support;

/// <summary>
/// A scripted HttpMessageHandler (same idea as the main app's test helper): queue responses or exceptions in the order the
/// code under test calls. Every request is captured with its body read up front.
/// </summary>
public sealed class FakeHttpHandler : HttpMessageHandler
{
    public sealed record Captured(HttpMethod Method, Uri Uri, string? Body, IReadOnlyDictionary<string, string> Headers);

    private readonly Queue<Func<HttpRequestMessage, HttpResponseMessage>> _queue = new();

    public List<Captured> Requests { get; } = new();

    public FakeHttpHandler Enqueue(HttpStatusCode status, string body = "")
    {
        _queue.Enqueue(_ => new HttpResponseMessage(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") });
        return this;
    }

    public FakeHttpHandler EnqueueThrow(Exception ex)
    {
        _queue.Enqueue(_ => throw ex);
        return this;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
        var headers = request.Headers.ToDictionary(h => h.Key, h => string.Join(",", h.Value), StringComparer.OrdinalIgnoreCase);
        Requests.Add(new Captured(request.Method, request.RequestUri!, body, headers));
        if (_queue.Count == 0) throw new InvalidOperationException($"FakeHttpHandler: no response queued for {request.Method} {request.RequestUri}");
        return _queue.Dequeue()(request);
    }
}
