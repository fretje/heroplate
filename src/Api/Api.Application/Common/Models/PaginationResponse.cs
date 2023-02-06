namespace Heroplate.Api.Application.Common.Models;

public class PaginationResponse<T>
{
    public List<T> Data { get; set; }

    public int TotalCount { get; set; }

    public int TotalCountWithoutFilter { get; set; }

    public PaginationResponse(List<T> data, int totalCount, int totalCountWithoutFilter = 0)
    {
        Data = data;
        TotalCount = totalCount;
        TotalCountWithoutFilter = totalCountWithoutFilter;
    }
}