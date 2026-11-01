namespace BodegaApp.Core.Pagination;

public class PaginationRequest
{
    private const int MaxRecordsPerPage = 50;
    private int _page = 1;
    private int _recordsPerPage = Env.DefaultPageSize;

    public string? Filter { get; set; }

    public int Page
    {
        get => _page;
        set => _page = value > 0 ? value : 1;
    }

    public int RecordsPerPage
    {
        get => _recordsPerPage;
        set => _recordsPerPage = value is > 0 and <= MaxRecordsPerPage ? value : Env.DefaultPageSize;
    }

    public static PaginationRequest Default => new();
}
