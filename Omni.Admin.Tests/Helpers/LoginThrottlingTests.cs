using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Omni.Admin.Common.Helpers;

namespace Omni.Admin.Tests.Helpers;

/// <summary>QA t6-admin-12: sign-in attempts are limited per network instead of locking the admin's account.</summary>
public class LoginThrottlingTests
{
    private static HttpContext Request(string method, string path, string? ip = "203.0.113.7", string? edgeIp = null, string? header = "CF-Connecting-IP")
    {
        var settings = new Dictionary<string, string?> { ["Proxy:ClientIpHeader"] = header };
        var services = new ServiceCollection()
            .AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(settings).Build())
            .BuildServiceProvider();
        var http = new DefaultHttpContext { RequestServices = services };
        http.Request.Method = method;
        http.Request.Path = path;
        if (ip is not null) http.Connection.RemoteIpAddress = IPAddress.Parse(ip);
        if (edgeIp is not null) http.Request.Headers["CF-Connecting-IP"] = edgeIp;
        return http;
    }

    [Fact]
    public void Only_a_sign_in_post_is_limited()
    {
        Assert.Equal("unlimited", LoginThrottling.Partition(Request("GET", "/Login")).PartitionKey);
        Assert.Equal("unlimited", LoginThrottling.Partition(Request("POST", "/Clients")).PartitionKey);
        Assert.Equal("login:203.0.113.7", LoginThrottling.Partition(Request("POST", "/Login")).PartitionKey);
        Assert.Equal("login:203.0.113.7", LoginThrottling.Partition(Request("POST", "/login")).PartitionKey);
    }

    [Fact]
    public void The_edge_header_names_the_visitor_when_configured()
    {
        Assert.Equal("198.51.100.4", LoginThrottling.ClientIp(Request("POST", "/Login", edgeIp: "198.51.100.4")));
        Assert.Equal("203.0.113.7", LoginThrottling.ClientIp(Request("POST", "/Login", edgeIp: "not-an-ip")));
        Assert.Equal("203.0.113.7", LoginThrottling.ClientIp(Request("POST", "/Login", edgeIp: "198.51.100.4", header: "")));
    }

    [Fact]
    public void Ten_attempts_per_network_pass_and_the_eleventh_is_refused()
    {
        var partition = LoginThrottling.Partition(Request("POST", "/Login"));
        using var limiter = partition.Factory(partition.PartitionKey);

        for (var i = 0; i < LoginThrottling.AttemptsPerWindow; i++)
        {
            using var lease = limiter.AttemptAcquire();
            Assert.True(lease.IsAcquired);
        }
        using var refused = limiter.AttemptAcquire();
        Assert.False(refused.IsAcquired);
        Assert.Equal((10, TimeSpan.FromMinutes(15)), (LoginThrottling.AttemptsPerWindow, LoginThrottling.Window));
    }
}
