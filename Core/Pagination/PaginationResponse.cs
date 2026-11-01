using BodegaApp.Core.Pagination.Abstractions;

namespace BodegaApp.Core.Pagination;

public class PaginationResponse<T> : IPagination
{
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int RecordsPerPage { get; set; }
    public string? Filter { get; set; }
    public int TotalCount { get; set; }
    public PagedList<T> List { get; set; } = new();
    public Dictionary<string, string> RouteValues { get; set; } = new();

    public int VisiblePages { get; set; } = 5;

    public List<int> Pages
    {
        get
        {
            var pages = new List<int>();
            var half = VisiblePages / 2;
            var start = CurrentPage - half + 1 - (VisiblePages % 2);
            var end = CurrentPage + half;
            var visible = Math.Min(VisiblePages, TotalPages);

            if (start <= 0)
            {
                start = 1;
                end = visible;
            }

            if (end > TotalPages)
            {
                start = TotalPages - visible + 1;
                end = TotalPages;
            }

            for (var page = start; page <= end; page++)
            {
                pages.Add(page);
            }

            return pages;
        }
    }

    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;
}
