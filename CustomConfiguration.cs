using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using BodegaApp.Core;
using BodegaApp.Data;
using BodegaApp.Domain.Entities;
using BodegaApp.Helpers;
using BodegaApp.Services.Abstractions;
using BodegaApp.Services.Implementations;

namespace BodegaApp;

public static class CustomConfiguration
{
    public static IServiceCollection AddBodegaConfiguration(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddDbContext<BodegaDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddIdentity<User, IdentityRole>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
        })
        .AddEntityFrameworkStores<BodegaDbContext>()
        // El seed genera el token con el que se sincroniza la contraseña del administrador
        .AddDefaultTokenProviders();

        // Si cambia el security stamp (logout), la cookie vieja deja de servir
        services.Configure<SecurityStampValidatorOptions>(options =>
        {
            options.ValidationInterval = TimeSpan.FromMinutes(5);
        });

        services.AddAutoMapper(configuration => { }, typeof(AutoMapperProfiles));

        services.AddScoped<ISuppliersService, SuppliersService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IUsersService, UsersService>();
        services.AddScoped<IRolesService, RolesService>();
        services.AddScoped<IHomeService, HomeService>();
        services.AddScoped<IAuthHelper, AuthHelper>();

        // El filtro de permisos y las vistas leen el usuario de la sesión
        services.AddHttpContextAccessor();

        // El túnel de Cloudflare termina el TLS y reenvía HTTP, así que sin esto la app no ve X-Forwarded-Proto
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

            // Cloudflare usa IPs dinámicas; la app solo escucha en 127.0.0.1 detrás del túnel
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.Cookie.SecurePolicy = environment.IsDevelopment()
                ? CookieSecurePolicy.SameAsRequest
                : CookieSecurePolicy.Always;
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
        });

        services.AddControllersWithViews();

        services.AddHsts(options =>
        {
            options.MaxAge = TimeSpan.FromDays(365);
            options.IncludeSubDomains = true;
        });

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // En memoria: con varias instancias cada una llevaría su propia cuenta
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

        return services;
    }
}
