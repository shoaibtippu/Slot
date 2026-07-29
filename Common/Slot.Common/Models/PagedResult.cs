using Slot.Common.Results;

namespace Slot.Common.Models;

public class PagedResult<TEntity>
{
    public IReadOnlyList<TEntity> Data { get; init; } = [];
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }

    public static PagedResult<TEntity> Success(IReadOnlyList<TEntity> data, int totalCount, int pageNumber, int pageSize)
        => new PagedResult<TEntity>
        {
            Data = data,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

    public static Result<PagedResult<TEntity>> PagedSuccess(IReadOnlyList<TEntity> data, int totalCount, int pageNumber, int pageSize)
        => Result.Success(new PagedResult<TEntity>
        {
            Data = data,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        });

    public static Result<PagedResult<TEntity>> Failure(Error error)
        => Result.Failure<PagedResult<TEntity>>(error);
}