using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SculptFlowAdmin.Business.Contracts.HttpClients.MainApp;
using SculptFlowAdmin.Business.Contracts.Managers;
using SculptFlowAdmin.Business.Contracts.Services.Auth;
using SculptFlowAdmin.Business.Contracts.Services.Channels;
using SculptFlowAdmin.Business.Contracts.Services.Clinics;
using SculptFlowAdmin.Business.Contracts.Services.Content;
using SculptFlowAdmin.Business.Contracts.Services.Leads;
using SculptFlowAdmin.Business.Contracts.Services.Overview;
using SculptFlowAdmin.Business.Contracts.Services.Staff;
using SculptFlowAdmin.Business.Engines.Admins;
using SculptFlowAdmin.Business.HttpClients.MainApp;
using SculptFlowAdmin.Business.Managers;
using SculptFlowAdmin.Business.Services.Auth;
using SculptFlowAdmin.Business.Services.Channels;
using SculptFlowAdmin.Business.Services.Clinics;
using SculptFlowAdmin.Business.Services.Content;
using SculptFlowAdmin.Business.Services.Leads;
using SculptFlowAdmin.Business.Services.Overview;
using SculptFlowAdmin.Business.Services.Staff;
using SculptFlowAdmin.Entities.Models;
using SculptFlowAdmin.Persistence.Contexts;
using SculptFlowAdmin.Persistence.Contracts;
using SculptFlowAdmin.Persistence.Contracts.Admins;
using SculptFlowAdmin.Persistence.Contracts.Channels;
using SculptFlowAdmin.Persistence.Contracts.Clinics;
using SculptFlowAdmin.Persistence.Contracts.Content;
using SculptFlowAdmin.Persistence.Contracts.Leads;
using SculptFlowAdmin.Persistence.Contracts.Overview;
using SculptFlowAdmin.Persistence.Contracts.Staff;
using SculptFlowAdmin.Persistence.Repositories;
using SculptFlowAdmin.Persistence.Repositories.Admins;
using SculptFlowAdmin.Persistence.Repositories.Channels;
using SculptFlowAdmin.Persistence.Repositories.Clinics;
using SculptFlowAdmin.Persistence.Repositories.Content;
using SculptFlowAdmin.Persistence.Repositories.Leads;
using SculptFlowAdmin.Persistence.Repositories.Overview;
using SculptFlowAdmin.Persistence.Repositories.Staff;

var builder = WebApplication.CreateBuilder(args);

// Same database as the main app (ConnectionStrings:Postgres, same key name as SculptFlowApp).
var connectionString = builder.Configuration.GetConnectionString("Postgres");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:Postgres is not set. Use `dotnet user-secrets set ConnectionStrings:Postgres \"...\"` locally " +
        "or the ConnectionStrings__Postgres environment variable when hosted.");
}

builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseNpgsql(connectionString));
builder.Services.AddDbContext<AdminDbContext>(o => o.UseNpgsql(connectionString));

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IPasswordHasher<AdminUser>, PasswordHasher<AdminUser>>();
builder.Services.AddScoped<IPasswordHasher<IdentityUser>, PasswordHasher<IdentityUser>>();
// Persistence: repositories + units of work (main-app tables via ApplicationDbContext, portal tables via AdminDbContext).
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAdminUnitOfWork, AdminUnitOfWork>();
builder.Services.AddScoped<IAdminUserRepository, AdminUserRepository>();
builder.Services.AddScoped<IAdminAuditRepository, AdminAuditRepository>();
builder.Services.AddScoped<IClinicAdminRepository, ClinicAdminRepository>();
builder.Services.AddScoped<IStaffAdminRepository, StaffAdminRepository>();
builder.Services.AddScoped<ILeadAdminRepository, LeadAdminRepository>();
builder.Services.AddScoped<IChannelAdminRepository, ChannelAdminRepository>();
builder.Services.AddScoped<IContentAdminRepository, ContentAdminRepository>();
builder.Services.AddScoped<IOverviewAdminRepository, OverviewAdminRepository>();

builder.Services.AddScoped<IAdminAuthService, AdminAuthService>();
builder.Services.AddScoped<IAdminAudit, AdminAudit>();
builder.Services.AddScoped<IClinicAdminService, ClinicAdminService>();
builder.Services.AddScoped<IStaffAdminService, StaffAdminService>();
builder.Services.AddScoped<ILeadAdminService, LeadAdminService>();
builder.Services.AddScoped<IChannelAdminService, ChannelAdminService>();
builder.Services.AddScoped<IContentAdminService, ContentAdminService>();
builder.Services.AddScoped<IOverviewAdminService, OverviewAdminService>();

// Billing is owned by the main app: the portal calls its platform-admin API (MainApp:ApiBaseUrl / PublicBaseUrl +
// MainApp:PlatformAdminApiKey) and never writes the billing.* tables itself.
builder.Services.Configure<SculptFlowAdmin.Common.Configs.MainAppApiOptions>(builder.Configuration.GetSection("MainApp"));
builder.Services.AddHttpClient<IBillingApiClient, BillingApiClient>(c => c.Timeout = TimeSpan.FromSeconds(30));

// Admin-only cookie. Its own name, so it can never be confused with the main app's staff cookie.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.Cookie.Name = ".SculptFlowAdmin.Auth";
        o.Cookie.HttpOnly = true;
        o.Cookie.SameSite = SameSiteMode.Strict;
        o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        o.ExpireTimeSpan = TimeSpan.FromHours(8);
        o.SlidingExpiration = true;
        o.LoginPath = "/Login";
        o.LogoutPath = "/Logout";
        o.AccessDeniedPath = "/Login";
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

builder.Services.AddRazorPages();
builder.Services.AddControllers();

var app = builder.Build();

// `dotnet run -- create-admin <email> "<full name>"` creates an admin (password from ADMIN_PASSWORD or typed in).
// The only way to make the first admin; later ones can be added from the Admins page.
if (args.Length > 0 && args[0] == "create-admin")
{
    await AdminCli.CreateAdminAsync(app.Services, args);
    return;
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();
app.MapControllers();

app.Run();
