using System;

namespace DatingApp.Application.Helpers;


public class PaginatedResult<T>
{
    public PaginationMetadata Metadata { get; set; } = default!;
    public List<T> Items { get; set; }

    public PaginatedResult(List<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        Metadata = new PaginationMetadata(totalCount, pageSize, pageNumber);
    }

    public static PaginatedResult<T> Empty(int pageNumber, int pageSize)
    {
        return new PaginatedResult<T>(new List<T>(), 0, pageNumber, pageSize);
    }
}
public class PaginationMetadata
{
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }

    public PaginationMetadata(int totalCount, int pageSize, int pageNumber)
    {
        TotalCount = totalCount;
        PageSize = pageSize;
        CurrentPage = pageNumber;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    }
};