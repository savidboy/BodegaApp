using BodegaApp.Core.Pagination;

namespace BodegaApp.Core.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> Paginate<T>(this IQueryable<T> query, PaginationRequest request) =>
        query.Skip((request.Page - 1) * request.RecordsPerPage)
             .Take(request.RecordsPerPage);
}
