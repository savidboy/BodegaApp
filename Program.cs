using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using BodegaApp.Data;
using BodegaApp.Domain.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BodegaDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<User, IdentityRole>(opt =>
{
    opt.Password.RequireDigit = false;
    opt.Password.RequiredLength = 6;
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<BodegaDbContext>()
// Lo necesita el seed para generar el token de reseteo de contraseña
.AddDefaultTokenProviders();

// Ngrok termina el TLS y reenvía HTTP, así que sin esto la app no ve X-Forwarded-Proto
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    // Ngrok usa IPs dinámicas, no hay lista fija de proxies; la app solo escucha en 127.0.0.1
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.ConfigureApplicationCookie(o =>
{
    o.LoginPath = "/Account/Login";

    // En desarrollo se permite HTTP; en producción la cookie solo viaja por HTTPS
    o.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    o.Cookie.HttpOnly = true;
    o.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddControllersWithViews();

// HSTS por un año
builder.Services.AddHsts(o =>
{
    o.MaxAge = TimeSpan.FromDays(365);
    o.IncludeSubDomains = true;
});

// Limite de intentos de login: 10 cada 15 minutos por IP
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // En memoria: si algún día hay varias instancias, cada una contaría por su lado
    options.AddPolicy("login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocida",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(15),
                QueueLimit = 0
            }));
});

var app = builder.Build();

// Migraciones y seed de datos iniciales
await DbInitializer.SeedAsync(app.Services);

// Antes de todo lo que dependa del esquema o del host
app.UseForwardedHeaders();

// Cabeceras de seguridad en todas las respuestas
app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;

    // Los iconos de Bootstrap son SVG en data:, por eso img-src los permite
    headers["Content-Security-Policy"] =
        "default-src 'self'; script-src 'self'; style-src 'self'; img-src 'self' data:; " +
        "font-src 'self'; object-src 'none'; base-uri 'self'; frame-ancestors 'none'";
    headers["X-Content-Type-Options"] = "nosniff";
    headers["X-Frame-Options"] = "DENY";
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    // Filtro obsoleto: se deja en 0, la proteccion real es el CSP
    headers["X-XSS-Protection"] = "0";

    await next();
});

if (!app.Environment.IsDevelopment())
{
    // Sin esto, un error muestra el stack trace al usuario final
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
