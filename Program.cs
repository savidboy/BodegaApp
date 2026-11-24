using Microsoft.AspNetCore.HttpOverrides;
using BodegaApp;
using BodegaApp.Data.Seeders;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddBodegaConfiguration(builder.Configuration, builder.Environment);

WebApplication app = builder.Build();

await SeedDb.SeedAsync(app.Services, app.Environment);

app.UseForwardedHeaders();

// Sin CSP un XSS puede cargar script desde cualquier dominio
app.Use(async (context, next) =>
{
    IHeaderDictionary headers = context.Response.Headers;

    headers["Content-Security-Policy"] =
        "default-src 'self'; script-src 'self'; style-src 'self'; img-src 'self' data:; " +
        "font-src 'self'; object-src 'none'; base-uri 'self'; frame-ancestors 'none'";
    headers["X-Content-Type-Options"] = "nosniff";
    headers["X-Frame-Options"] = "DENY";
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    headers["X-XSS-Protection"] = "0";

    await next();
});

if (!app.Environment.IsDevelopment())
{
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
