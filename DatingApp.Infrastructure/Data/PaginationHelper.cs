using DatingApp.Application.Helpers;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Infrastructure.Data;

public class PaginationHelper
{
    public static async Task<PaginatedResult<T>> CreateAsync<T>(IQueryable<T> query,
        int pageNumber, int pageSize) where T : class
    {
        var count = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResult<T>
        {
            Metadata = new PaginationMetadata
            {
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling(count / (double)pageSize),
                TotalCount = count,
                PageSize = pageSize
            },
            Items = items
        };
    }
}
