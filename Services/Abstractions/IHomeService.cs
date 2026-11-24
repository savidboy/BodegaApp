using BodegaApp.Core;
using BodegaApp.DTOs.Home;

namespace BodegaApp.Services.Abstractions;

public interface IHomeService
{
    Task<Response<HomeDashboardDTO>> GetDashboardAsync();
}
