namespace Heroplate.Api.Application.Common.Models;

public static class PaginationResponseExtensions
{
    public static async Task<PaginationResponse<TDestination>> PaginatedListAsync<T, TDestination>(
        this IReadRepositoryBase<T> repository, ISpecification<T, TDestination> spec, CancellationToken ct)
        where T : class
        where TDestination : class
    {
        var list = await repository.ListAsync(spec, ct);
        int count = await repository.CountAsync(spec, ct);

        return new PaginationResponse<TDestination>(list, count);
    }
}