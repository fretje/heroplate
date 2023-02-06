namespace Heroplate.Admin.Application.Common;

public static class PaginationFilterExtensions
{
    public static void FillWith(this PaginationFilter filter, TableState state)
    {
        filter.PageSize = state.PageSize;
        filter.PageNumber = state.Page + 1;
        filter.OrderBy = string.IsNullOrEmpty(state.SortLabel)
            ? Array.Empty<string>()
            : state.SortDirection == SortDirection.None
                ? Array.Empty<string>()
                : state.SortDirection == SortDirection.Ascending
                    ? new[] { $"{state.SortLabel}" }
                    : new[] { $"{state.SortLabel} {state.SortDirection}" };
    }
}