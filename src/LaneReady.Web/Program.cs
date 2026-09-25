using LaneReady.Application;
using LaneReady.Infrastructure;
using LaneReady.RulesEngine;
using LaneReady.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Identity.Web;
using MudBlazor.Services;
using System.Security.Claims;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ── Services ──────────────────────────────────────────────────────────────────

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();

// Application + Infrastructure layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddRulesEngine();

// Authentication — dev uses auto-signed-in cookie; production uses Entra External ID
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
        });
}
else
{
    builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));
}

// Claims transformation: links Entra identity to LaneReady User/Organisation
builder.Services.AddTransient<Microsoft.AspNetCore.Authentication.IClaimsTransformation,
    TenantClaimsTransformation>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAuthenticated", p => p.RequireAuthenticatedUser());
    options.AddPolicy("RequireReadOnly", p =>
        p.RequireAuthenticatedUser()
         .RequireClaim("user_role"));
    options.AddPolicy("RequireOperations", p =>
        p.RequireAuthenticatedUser()
         .RequireAssertion(ctx =>
         {
             var role = ctx.User.FindFirst("user_role")?.Value;
             return role is "Owner" or "Operations" or "Admin";
         }));
    options.AddPolicy("RequireOwner", p =>
        p.RequireAuthenticatedUser()
         .RequireAssertion(ctx =>
         {
             var role = ctx.User.FindFirst("user_role")?.Value;
             return role is "Owner" or "Admin";
         }));
    options.AddPolicy("RequireAdmin", p =>
        p.RequireAuthenticatedUser()
         .RequireAssertion(ctx =>
             ctx.User.FindFirst("user_role")?.Value == "Admin"));
});

// Rate limiting (OWASP A05)
builder.Services.AddRateLimiter(options =>
{
    // Public eligibility checker: 10 req/min per IP
    options.AddFixedWindowLimiter("eligibility", o =>
    {
        o.PermitLimit = 10;
        o.Window = TimeSpan.FromMinutes(1);
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        o.QueueLimit = 2;
    });

    // Auth endpoints: 5 req/min per IP
    options.AddFixedWindowLimiter("auth", o =>
    {
        o.PermitLimit = 5;
        o.Window = TimeSpan.FromMinutes(1);
        o.QueueLimit = 0;
    });

    // Authenticated API endpoints: 100 req/min per user
    options.AddFixedWindowLimiter("api", o =>
    {
        o.PermitLimit = 100;
        o.Window = TimeSpan.FromMinutes(1);
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        o.QueueLimit = 10;
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// Application Insights
builder.Services.AddApplicationInsightsTelemetry();

// ── Pipeline ──────────────────────────────────────────────────────────────────

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Security headers (OWASP A05)
app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers["X-Frame-Options"] = "DENY";
    headers["X-Content-Type-Options"] = "nosniff";
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    headers["Permissions-Policy"] = "geolocation=(), camera=(), microphone=()";

    // CSP: Blazor requires unsafe-inline for its script bootstrapper
    headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline'; " +
        "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
        "font-src 'self' https://fonts.gstatic.com; " +
        "img-src 'self' data:; " +
        "connect-src 'self' wss:; " +
        "frame-ancestors 'none'";

    if (!app.Environment.IsDevelopment())
        headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";

    await next();
});

app.UseRateLimiter();
app.UseStaticFiles();
app.UseRouting();

// In Development: auto-sign-in a dev user so the app works without Entra External ID
if (app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "dev-sub-00000000-0000-0000-0000-000000000001"),
                new(ClaimTypes.Email, "dev@laneready.local"),
                new(ClaimTypes.Name, "Dev User"),
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            context.User = principal;
        }
        await next();
    });
}

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapControllers();
app.MapRazorComponents<LaneReady.Web.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();
