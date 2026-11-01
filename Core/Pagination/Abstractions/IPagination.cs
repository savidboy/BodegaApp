namespace BodegaApp.Core.Pagination.Abstractions;

public interface IPagination
{
    int CurrentPage { get; set; }
    int TotalPages { get; set; }
    int RecordsPerPage { get; set; }
    string? Filter { get; set; }
    int TotalCount { get; set; }
    List<int> Pages { get; }
    bool HasPrevious { get; }
    bool HasNext { get; }
    Dictionary<string, string> RouteValues { get; set; }
}
