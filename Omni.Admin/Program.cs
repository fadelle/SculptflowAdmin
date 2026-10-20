using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Omni.Admin.Business.Contracts.HttpClients.MainApp;
using Omni.Admin.Business.Contracts.Managers;
using Omni.Admin.Business.Contracts.Services.Admins;
using Omni.Admin.Business.Contracts.Services.Auth;
using Omni.Admin.Business.Contracts.Services.Channels;
using Omni.Admin.Business.Contracts.Services.BusinessTypes;
using Omni.Admin.Business.Contracts.Services.Clients;
using Omni.Admin.Business.Contracts.Services.Content;
using Omni.Admin.Business.Contracts.Services.KnowledgeBenchmark;
using Omni.Admin.Business.Contracts.Services.Contacts;
using Omni.Admin.Business.Contracts.Services.Overview;
using Omni.Admin.Business.Contracts.Services.Members;
using Omni.Admin.Business.HttpClients.MainApp;
using Omni.Admin.Business.Services.Billing;
using Omni.Admin.Business.Services.Configuration;
using Omni.Admin.Business.Services.Caching;
using Omni.Admin.Business.Contracts.Services.Caching;
using Omni.Admin.Business.Contracts.Services.Monitoring;
using Omni.Admin.Business.Services.Monitoring;
using Omni.Admin.Business.Contracts.Services.Configuration;
using Omni.Admin.Business.Contracts.Services.Billing;
using Omni.Admin.Business.Managers;
using Omni.Admin.Business.Services.Admins;
using Omni.Admin.Business.Services.Auth;
using Omni.Admin.Business.Services.Channels;
using Omni.Admin.Business.Services.BusinessTypes;
using Omni.Admin.Business.Services.Clients;
using Omni.Admin.Business.Services.Content;
using Omni.Admin.Business.Services.KnowledgeBenchmark;
using Omni.Admin.Business.Services.Contacts;
using Omni.Admin.Business.Services.Overview;
using Omni.Admin.Business.Services.Members;
using Omni.Admin.Common.Configs;
using Omni.Admin.Persistence.Contexts;
using Omni.Admin.Persistence.Contracts;
using Omni.Admin.Persistence.Contracts.Admins;
using Omni.Admin.Persistence.Repositories;
using Omni.Admin.Persistence.Repositories.Admins;
using Omni.Admin.Pages.Shared;

var builder = WebApplication.CreateBuilder(args);

// The portal's own table (admin.admin_audit_log) lives in the same database as the main app (ConnectionStrings:Postgres, same key
// name as SculptFlowApp); it never touches the main app's tables, only the main app's platform-admin APIs.
var connectionString = builder.Configuration.GetConnectionString("Postgres");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:Postgres is not set. Use `dotnet user-secrets set ConnectionStrings:Postgres \"...\"` locally " +
        "or the ConnectionStrings__Postgres environment variable when hosted.");
}

builder.Services.AddDbContext<AdminDbContext>(o => o.UseNpgsql(connectionString));

builder.Services.AddHttpContextAccessor();
// Persistence: only the portal's own table (admin.admin_audit_log). Everything else, admin users included, goes through the
// main app's APIs.
builder.Services.AddScoped<IAdminUnitOfWork, AdminUnitOfWork>();
builder.Services.AddScoped<IAdminAuditRepository, AdminAuditRepository>();

builder.Services.AddScoped<IAdminAudit, AdminAudit>();
builder.Services.AddScoped<IClientAdminService, ClientAdminService>();
builder.Services.AddScoped<IBusinessTypeAdminService, BusinessTypeAdminService>();
builder.Services.AddScoped<IMemberAdminService, MemberAdminService>();
builder.Services.AddScoped<IContactAdminService, ContactAdminService>();
builder.Services.AddScoped<IChannelAdminService, ChannelAdminService>();
builder.Services.AddScoped<IContentAdminService, ContentAdminService>();
builder.Services.AddScoped<IOverviewAdminService, OverviewAdminService>();

// Billing is owned by the main app: the portal calls its platform-admin API (MainApp:ApiBaseUrl / PublicBaseUrl +
// MainApp:PlatformAdminApiKey) and never reads or writes the billing.* tables itself. Pages call IBillingAdminService,
// which calls the API client and records each write in admin_audit_log.
builder.Services.Configure<Omni.Admin.Common.Configs.MainAppApiOptions>(builder.Configuration.GetSection("MainApp"));
builder.Services.AddHttpClient<IBillingApiClient, BillingApiClient>(c => c.Timeout = TimeSpan.FromSeconds(30));
builder.Services.AddScoped<IBillingAdminService, BillingAdminService>();
// The main app's configuration overrides (config.settings), through its platform-admin settings API.
builder.Services.AddHttpClient<ISettingsApiClient, SettingsApiClient>(c => c.Timeout = TimeSpan.FromSeconds(30));
builder.Services.AddScoped<ISettingsAdminService, SettingsAdminService>();
builder.Services.AddHttpClient<ICacheApiClient, CacheApiClient>(c => c.Timeout = TimeSpan.FromSeconds(30));
builder.Services.AddScoped<ICacheAdminService, CacheAdminService>();
// The main app's per-route API timings (read-only API timings page).
builder.Services.AddHttpClient<IApiTimingsApiClient, ApiTimingsApiClient>(c => c.Timeout = TimeSpan.FromSeconds(30));
builder.Services.AddScoped<IApiTimingsAdminService, ApiTimingsAdminService>();
builder.Services.AddHttpClient<IAppLogsApiClient, AppLogsApiClient>(c => c.Timeout = TimeSpan.FromSeconds(30));
builder.Services.AddScoped<IAppLogsAdminService, AppLogsAdminService>();
builder.Services.AddHttpClient<IMessageTracesApiClient, MessageTracesApiClient>(c => c.Timeout = TimeSpan.FromSeconds(30));
builder.Services.AddScoped<IMessageTracesAdminService, MessageTracesAdminService>();
// Clients, channels, member, contacts, content and the dashboard: the main app's platform-admin APIs (reads and writes).
builder.Services.AddHttpClient<IBusinessTypesApiClient, BusinessTypesApiClient>(c => c.Timeout = TimeSpan.FromSeconds(30));
builder.Services.AddHttpClient<IClientsApiClient, ClientsApiClient>(c => c.Timeout = TimeSpan.FromSeconds(30));
builder.Services.AddHttpClient<IChannelsApiClient, ChannelsApiClient>(c => c.Timeout = TimeSpan.FromSeconds(60));
builder.Services.AddHttpClient<IMembersApiClient, MembersApiClient>(c => c.Timeout = TimeSpan.FromSeconds(30));
builder.Services.AddHttpClient<IContactsApiClient, ContactsApiClient>(c => c.Timeout = TimeSpan.FromSeconds(30));
builder.Services.AddHttpClient<IContentApiClient, ContentApiClient>(c => c.Timeout = TimeSpan.FromSeconds(30));
builder.Services.AddHttpClient<IOverviewApiClient, OverviewApiClient>(c => c.Timeout = TimeSpan.FromSeconds(30));
// A client's Knowledge Retrieval Benchmark (Search benchmark page). Generate waits for the main app to hand the chunks to n8n.
builder.Services.AddHttpClient<IKnowledgeBenchmarkApiClient, KnowledgeBenchmarkApiClient>(c => c.Timeout = TimeSpan.FromSeconds(120));
builder.Services.AddScoped<IKnowledgeBenchmarkAdminService, KnowledgeBenchmarkAdminService>();
// Admins are the main app's users (role admin): it checks sign-ins (auth) and keeps the accounts (admins).
builder.Services.AddHttpClient<IAuthApiClient, AuthApiClient>(c => c.Timeout = TimeSpan.FromSeconds(15));
builder.Services.AddHttpClient<IAdminsApiClient, AdminsApiClient>(c => c.Timeout = TimeSpan.FromSeconds(30));
builder.Services.AddScoped<IAdminAuthService, AdminAuthService>();
builder.Services.AddScoped<IAdminAccountService, AdminAccountService>();

// The header's global client scope (docs/UI_GUIDE.md §7): ScopePageFilter resolves it for every page (?clientId=, else
// the cookie); ClientScopeLookup names it from the client list, cached briefly.
builder.Services.Configure<ScopeOptions>(builder.Configuration.GetSection("Scope"));
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ScopeContext>();
builder.Services.AddScoped<IScopeLookup, ClientScopeLookup>();

// Admin-only cookie. Its own name, so it can never be confused with the main app's member cookie.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.Cookie.Name = "omni.admin";
        o.Cookie.HttpOnly = true;
        o.Cookie.SameSite = SameSiteMode.Strict;
        o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        o.ExpireTimeSpan = TimeSpan.FromHours(8);
        o.SlidingExpiration = true;
        o.LoginPath = "/Login";
        o.LogoutPath = "/Logout";
        o.AccessDeniedPath = "/Login";
        // Asks the main app (remembered for a minute) whether the admin is still an active admin; signs out if not.
        o.Events.OnValidatePrincipal = AdminAuthService.ValidatePrincipalAsync;
        // The admin API answers 401 instead of redirecting to the login page.
        o.Events.OnRedirectToLogin = ctx =>
        {
            if (ctx.Request.Path.StartsWithSegments("/api")) ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            else ctx.Response.Redirect(ctx.RedirectUri);
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization(o =>
{
    // Everything requires a signed-in admin unless it opts out with [AllowAnonymous].
    o.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
});

builder.Services.AddRazorPages().AddMvcOptions(o => o.Filters.Add<ScopePageFilter>());
builder.Services.AddControllers();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// An error status with no body (404 for an unknown address or a missing record, 400 for an expired form…) shows the
// styled Status page. The JSON API (/api) keeps its plain status codes.
app.UseWhen(ctx => !ctx.Request.Path.StartsWithSegments("/api"),
    branch => branch.UseStatusCodePagesWithReExecute("/Status", "?code={0}"));

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Static files (CSS, JS, logo) are public: the fallback policy would otherwise send the sign-in page's own stylesheet
// to the sign-in page.
app.MapStaticAssets().AllowAnonymous();
app.MapRazorPages().WithStaticAssets();
app.MapControllers();

app.Run();
