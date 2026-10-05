using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BodegaApp.Data;
using BodegaApp.Domain.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BodegaDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<Usuario, IdentityRole>(opt =>
{
    opt.Password.RequireDigit = false;
    opt.Password.RequiredLength = 6;
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<BodegaDbContext>();

builder.Services.ConfigureApplicationCookie(o =>
{
    o.LoginPath = "/Account/Login";
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Inicialización y seed de datos iniciales
await DbInitializer.SeedAsync(app.Services);

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();