using BodegaApp.Core.Pagination;

namespace BodegaApp.DTOs.Supplier;

public class SupplierPaginationRequest : PaginationRequest
{
    public bool IncludeInactive { get; set; }
}
