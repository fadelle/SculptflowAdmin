using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SculptFlowAdmin.Common.Configs;
using SculptFlowAdmin.Common.Exceptions;
using SculptFlowAdmin.Common.Statics;

namespace SculptFlowAdmin.Business.HttpClients.MainApp;

/// <summary>
/// Shared plumbing for the main app's platform-admin APIs (/api/platform-admin/{domain}): the shared key
/// (X-Platform-Admin-Key), the signed-in admin's email as X-Admin-Actor, an Idempotency-Key from a form's operation id on
/// money writes, and the main app's errors turned into messages an admin can read (MainAppApiException).
/// </summary>
public abstract class MainAppApiClient
{
    protected static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;
    private readonly MainAppApiOptions _options;
    private readonly IHttpContextAccessor _context;
    private readonly ILogger _logger;
    private readonly string _prefix;
    private readonly string _area;

    /// <param name="prefix">The API's path, e.g. "api/platform-admin/billing/".</param>
    /// <param name="area">What the admin calls this area in messages, e.g. "Billing".</param>
    protected MainAppApiClient(HttpClient http, IOptions<MainAppApiOptions> options, IHttpContextAccessor context, ILogger logger,
        string prefix, string area)
    {
        _http = http;
        _options = options.Value;
        _context = context;
        _logger = logger;
        _prefix = prefix;
        _area = area;
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_options.BaseUrl) && !string.IsNullOrWhiteSpace(_options.PlatformAdminApiKey);
    public string? BaseUrl => _options.BaseUrl;

    protected async Task<T?> GetAsync<T>(string path, CancellationToken ct) where T : class
    {
        using var response = await SendAsync(HttpMethod.Get, path, null, null, ct);
        if (response.StatusCode == HttpStatusCode.NotFound) return null; // no such record (or the API is off)
        await EnsureSuccessAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<T>(Json, ct);
    }

    /// <summary>For endpoints that always exist (lists, catalogs): a 404 can only mean the main app has the API off.</summary>
    protected async Task<T> GetRequiredAsync<T>(string path, CancellationToken ct) where T : class =>
        await GetAsync<T>(path, ct) ?? throw ApiOff();

    /// <summary>A write. A 404 means the record is gone, except for a create (<paramref name="isCreate"/>), where it can
    /// only mean the main app has the API off.</summary>
    protected async Task WriteAsync(HttpMethod method, string path, object? body, string? opId, bool isCreate, CancellationToken ct)
    {
        using var response = await SendWriteAsync(method, path, body, opId, isCreate, ct);
    }

    /// <summary>A write whose answer the caller needs (same 404 rules as <see cref="WriteAsync"/>).</summary>
    protected async Task<T> WriteForAsync<T>(HttpMethod method, string path, object? body, string? opId, bool isCreate, CancellationToken ct)
        where T : class
    {
        using var response = await SendWriteAsync(method, path, body, opId, isCreate, ct);
        return await response.Content.ReadFromJsonAsync<T>(Json, ct)
            ?? throw new MainAppApiException("The main app answered with an empty body.");
    }

    private async Task<HttpResponseMessage> SendWriteAsync(HttpMethod method, string path, object? body, string? opId, bool isCreate, CancellationToken ct)
    {
        string? key = null;
        if (opId is not null)
        {
            if (!Guid.TryParse(opId, out var op)) throw new MainAppApiException("This form expired. Reload the page and try again.");
            key = $"admin-portal:{op:N}";
        }
        var response = await SendAsync(method, path, body, key, ct);
        try
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                if (isCreate) throw ApiOff();
                throw new KeyNotFoundException();
            }
            await EnsureSuccessAsync(response, ct);
            return response;
        }
        catch
        {
            response.Dispose();
            throw;
        }
    }

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, object? body, string? idempotencyKey, CancellationToken ct)
    {
        if (!IsConfigured)
        {
            throw new MainAppApiException($"{_area} isn't connected to the main app yet: set MainApp:PlatformAdminApiKey (and MainApp:ApiBaseUrl " +
                "or MainApp:PublicBaseUrl) for this portal, and the same key as PlatformAdmin:ApiKey on the main app.");
        }

        var request = new HttpRequestMessage(method, new Uri(new Uri(_options.BaseUrl + "/"), _prefix + path));
        request.Headers.Add("X-Platform-Admin-Key", _options.PlatformAdminApiKey);
        var user = _context.HttpContext?.User;
        request.Headers.Add("X-Admin-Actor", user?.Identity?.IsAuthenticated == true ? AdminClaims.Email(user) : "admin-portal");
        if (idempotencyKey is not null) request.Headers.Add("Idempotency-Key", idempotencyKey);
        if (body is not null) request.Content = JsonContent.Create(body, body.GetType(), options: Json);
        else if (method != HttpMethod.Get) request.Content = JsonContent.Create(new { }, options: Json);

        try
        {
            return await _http.SendAsync(request, ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException && !ct.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "Main app API call {Method} {Path} failed.", method, _prefix + path);
            throw new MainAppApiException($"Couldn't reach the main app at {_options.BaseUrl}. Try again in a moment.");
        }
    }

    private async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode) return;
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new MainAppApiException("The main app rejected this portal's key: MainApp:PlatformAdminApiKey must equal the main app's PlatformAdmin:ApiKey.", response.StatusCode);

        var text = await response.Content.ReadAsStringAsync(ct);
        var message = ReadError(text);
        _logger.LogInformation("Main app API {Path} answered {Status}: {Message}", response.RequestMessage?.RequestUri?.AbsolutePath, (int)response.StatusCode, message);
        throw new MainAppApiException(message ?? $"The main app answered {(int)response.StatusCode} {response.ReasonPhrase}.", response.StatusCode);
    }

    /// <summary>{ "error": "…" } from the platform-admin controllers, or ASP.NET's validation problem details.</summary>
    private static string? ReadError(string text)
    {
        try
        {
            using var doc = JsonDocument.Parse(text);
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object) return null;
            if (root.TryGetProperty("error", out var e) && e.ValueKind == JsonValueKind.String) return e.GetString();
            if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
            {
                var all = errors.EnumerateObject().SelectMany(p => p.Value.ValueKind == JsonValueKind.Array
                    ? p.Value.EnumerateArray().Select(v => v.GetString()) : [p.Value.ToString()]).Where(s => !string.IsNullOrEmpty(s));
                var joined = string.Join(" ", all);
                if (joined.Length > 0) return joined;
            }
            if (root.TryGetProperty("title", out var t) && t.ValueKind == JsonValueKind.String) return t.GetString();
        }
        catch (JsonException)
        {
        }
        return null;
    }

    private static MainAppApiException ApiOff() =>
        new("The main app's platform-admin API is off: PlatformAdmin:ApiKey isn't set on the main app.", HttpStatusCode.NotFound);

    protected static string Esc(string value) => Uri.EscapeDataString(value.Trim());

    /// <summary>"?a=1&amp;b=x" from the non-empty values (empty string when none).</summary>
    protected static string Query(params (string Key, object? Value)[] parts)
    {
        var pairs = parts
            .Select(p => (p.Key, Value: p.Value switch
            {
                null => null,
                bool b => b ? "true" : null,
                int i => i > 0 ? i.ToString(System.Globalization.CultureInfo.InvariantCulture) : null,
                string s => string.IsNullOrWhiteSpace(s) ? null : s,
                var v => Convert.ToString(v, System.Globalization.CultureInfo.InvariantCulture),
            }))
            .Where(p => p.Value is not null)
            .Select(p => $"{p.Key}={Esc(p.Value!)}")
            .ToList();
        return pairs.Count == 0 ? "" : "?" + string.Join("&", pairs);
    }
}
