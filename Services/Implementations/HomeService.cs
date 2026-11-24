using Microsoft.EntityFrameworkCore;
using BodegaApp.Core;
using BodegaApp.Data;
using BodegaApp.DTOs.Home;
using BodegaApp.Services.Abstractions;

namespace BodegaApp.Services.Implementations;

public class HomeService : IHomeService
{
    private readonly BodegaDbContext _context;

    public HomeService(BodegaDbContext context)
    {
        _context = context;
    }

    public async Task<Response<HomeDashboardDTO>> GetDashboardAsync()
    {
        HomeDashboardDTO dashboard = new()
        {
            ActiveSuppliers = await _context.Suppliers.CountAsync(supplier => supplier.IsActive),
            InactiveSuppliers = await _context.Suppliers.CountAsync(supplier => !supplier.IsActive),
            Users = await _context.Users.CountAsync(),
            Roles = await _context.Roles.CountAsync(),
            Permissions = await _context.Permissions.CountAsync()
        };

        return Response<HomeDashboardDTO>.Success(dashboard);
    }
}
