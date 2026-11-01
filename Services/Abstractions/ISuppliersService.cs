using BodegaApp.Core;
using BodegaApp.Core.Pagination;
using BodegaApp.DTOs.Supplier;

namespace BodegaApp.Services.Abstractions;

public interface ISuppliersService
{
    Task<Response<PaginationResponse<SupplierDTO>>> GetPaginationAsync(SupplierPaginationRequest request);
    Task<Response<SupplierDTO>> GetOneAsync(int id, bool includeInactive);
    Task<Response<CreateSupplierDTO>> CreateAsync(CreateSupplierDTO dto);
    Task<Response<SupplierDTO>> UpdateAsync(UpdateSupplierDTO dto, bool includeInactive);
    Task<Response> DeactivateAsync(int id, bool includeInactive);
    Task<Response> DeleteAsync(int id);
}
