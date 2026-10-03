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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BodegaDbContext>();
    db.Database.EnsureCreated();

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();
    if (await userManager.FindByEmailAsync("admin@bodega.com") == null)
    {
        var admin = new Usuario { UserName = "admin@bodega.com", Email = "admin@bodega.com", NombreCompleto = "Admin" };
        await userManager.CreateAsync(admin, "admin123");
    }
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();