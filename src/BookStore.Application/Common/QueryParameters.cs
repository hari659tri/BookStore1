namespace BookStore.Application.Common;

public class QueryParameters
{
    private int _pageNumber = 1;
    private int _pageSize = 10;

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value is < 1 or > 100 ? 10 : value;
    }

    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public bool Desc { get; set; }
}

public sealed class BookQueryParameters : QueryParameters
{
    public int? CategoryId { get; set; }
    public int? AuthorId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}
