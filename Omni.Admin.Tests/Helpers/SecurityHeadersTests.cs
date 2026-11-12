using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Omni.Admin.Middleware;

namespace Omni.Admin.Tests.Helpers;

/// <summary>QA t6-admin-15: the portal sent no CSP, nosniff or referrer policy.</summary>
public class SecurityHeadersTests
{
    [Theory]
    [InlineData("/Login", false)]
    [InlineData("/api/admin/knowledge-benchmark/x", true)]
    public async Task Every_response_gets_the_security_headers(string path, bool api)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        var ran = false;
        await new SecurityHeadersMiddleware(_ => { ran = true; return Task.CompletedTask; }).InvokeAsync(context);

        var h = context.Response.Headers;
        Assert.True(ran);
        Assert.Equal("DENY", h.XFrameOptions.ToString());
        Assert.Equal("nosniff", h.XContentTypeOptions.ToString());
        Assert.Equal("same-origin", h["Referrer-Policy"].ToString());
        Assert.Contains("frame-ancestors 'none'", h.ContentSecurityPolicy.ToString());
        Assert.Equal(api ? "no-store" : "", h.CacheControl.ToString());
    }

    [Fact]
    public void Scripts_come_only_from_the_portal_itself()
    {
        var script = SecurityHeadersMiddleware.ContentSecurityPolicy.Split(';').Select(d => d.Trim()).Single(d => d.StartsWith("script-src"));
        Assert.Equal("script-src 'self'", script);
        Assert.Contains("object-src 'none'", SecurityHeadersMiddleware.ContentSecurityPolicy);
    }

    /// <summary>The CSP refuses inline scripts and handlers, so a page that adds one would silently stop working: none may.</summary>
    [Fact]
    public void No_page_has_an_inline_script_or_event_handler()
    {
        var pages = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Omni.Admin", "Pages"));
        Assert.True(Directory.Exists(pages), pages);
        var inlineScript = new Regex(@"<script(?![^>]*\bsrc=)(?![^>]*type=""application/json"")[^>]*>", RegexOptions.IgnoreCase);
        var handler = new Regex(@"\son[a-z]+\s*=\s*[""']", RegexOptions.IgnoreCase);
        var offenders = Directory.GetFiles(pages, "*.cshtml", SearchOption.AllDirectories)
            .SelectMany(f => File.ReadAllLines(f).Select((line, i) => (f, i, line)))
            .Where(x => inlineScript.IsMatch(x.line) || handler.IsMatch(x.line) || x.line.Contains("javascript:", StringComparison.OrdinalIgnoreCase))
            .Select(x => $"{Path.GetRelativePath(pages, x.f)}:{x.i + 1}: {x.line.Trim()}")
            .ToList();
        Assert.True(offenders.Count == 0, string.Join(Environment.NewLine, offenders));
    }
}
