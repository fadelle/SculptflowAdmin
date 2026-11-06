using System.Net;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Omni.Admin.Common.Helpers;

/// <summary>
/// Caps sign-in attempts (POST /Login) per visitor network: <see cref="AttemptsPerWindow"/> per <see cref="Window"/>. The main app
/// no longer locks an admin account after failed sign-ins (QA t6-admin-12: anyone could keep the real admin locked out for 15 minutes,
/// again and again), so this is what slows down password guessing, and it only ever blocks the guesser's own network. A refused
/// attempt goes back to the sign-in page with a message. The visitor IP is the header the edge sets (<c>Proxy:ClientIpHeader</c>,
/// CF-Connecting-IP behind Render's Cloudflare, like the main app), else the connection's address.
/// </summary>
public static class LoginThrottling
{
    public const int AttemptsPerWindow = 10;
    public static readonly TimeSpan Window = TimeSpan.FromMinutes(15);

    public const string RejectedMessage = "Too many sign-in attempts from your network. Wait 15 minutes and try again.";

    /// <summary>The query flag the sign-in page shows <see cref="RejectedMessage"/> for.</summary>
    public const string ThrottledQuery = "throttled";

    public static IServiceCollection AddLoginThrottling(this IServiceCollection services) =>
        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(Partition);
            options.OnRejected = (context, _) =>
            {
                var http = context.HttpContext;
                http.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(LoginThrottling))
                    .LogWarning("Admin sign-in attempts throttled for {Ip}.", ClientIp(http));
                http.Response.Redirect($"/Login?{ThrottledQuery}=1");
                return ValueTask.CompletedTask;
            };
        });

    /// <summary>A per-network window for a sign-in POST; no limit for anything else.</summary>
    public static RateLimitPartition<string> Partition(HttpContext context)
    {
        if (!IsSignInAttempt(context.Request)) return RateLimitPartition.GetNoLimiter("unlimited");
        return RateLimitPartition.GetFixedWindowLimiter("login:" + ClientIp(context), _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = AttemptsPerWindow,
            Window = Window,
            QueueLimit = 0,
            AutoReplenishment = true
        });
    }

    public static bool IsSignInAttempt(HttpRequest request) =>
        HttpMethods.IsPost(request.Method) && request.Path.Equals("/Login", StringComparison.OrdinalIgnoreCase);

    /// <summary>The visitor's IP: the trusted edge header when configured and present, else the connection's address.</summary>
    public static string ClientIp(HttpContext context)
    {
        var header = context.RequestServices.GetService<IConfiguration>()?["Proxy:ClientIpHeader"];
        if (!string.IsNullOrWhiteSpace(header)
            && context.Request.Headers.TryGetValue(header, out var values)
            && values.ToString().Split(',')[0].Trim() is { Length: > 0 } first
            && IPAddress.TryParse(first, out var address))
        {
            return address.ToString();
        }
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
