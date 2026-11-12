namespace Omni.Admin.Middleware;

/// <summary>
/// Security headers on every portal response (QA t6-admin-15: only antiforgery's X-Frame-Options was sent). Set before the
/// rest of the pipeline runs; antiforgery only adds X-Frame-Options when it is missing, so DENY stays.
/// <list type="bullet">
/// <item>No framing: X-Frame-Options DENY and CSP frame-ancestors 'none'.</item>
/// <item>X-Content-Type-Options nosniff, Referrer-Policy same-origin (portal addresses carry client ids), a Permissions-Policy
/// that turns off camera, microphone, location, payment and USB.</item>
/// <item>A strict Content-Security-Policy: scripts only from the portal itself, with no inline script or handler (the theme
/// start-up is wwwroot/js/theme-init.js; aurora.js handles data-history-back / data-autosubmit). Styles allow inline
/// style attributes, which the pages use for layout, plus Google Fonts; fonts from Google Fonts' file host. Nothing
/// else: no plugins, no foreign base URL, forms post only to the portal.</item>
/// <item>/api answers are never stored by a browser or proxy (Cache-Control no-store).</item>
/// </list>
/// HSTS is set by UseHsts outside Development. A new external script, font or image host must be added here.
/// </summary>
public sealed class SecurityHeadersMiddleware
{
    public const string ContentSecurityPolicy =
        "default-src 'self'; " +
        "script-src 'self'; " +
        "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
        "font-src 'self' https://fonts.gstatic.com; " +
        "img-src 'self' data:; " +
        "connect-src 'self'; " +
        "object-src 'none'; " +
        "base-uri 'self'; " +
        "form-action 'self'; " +
        "frame-ancestors 'none'";

    public const string PermissionsPolicy = "camera=(), microphone=(), geolocation=(), payment=(), usb=()";

    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next) => _next = next;

    public Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;
        headers.XFrameOptions = "DENY";
        headers.XContentTypeOptions = "nosniff";
        headers["Referrer-Policy"] = "same-origin";
        headers["Permissions-Policy"] = PermissionsPolicy;
        headers.ContentSecurityPolicy = ContentSecurityPolicy;
        if (context.Request.Path.StartsWithSegments("/api")) headers.CacheControl = "no-store";
        return _next(context);
    }
}
