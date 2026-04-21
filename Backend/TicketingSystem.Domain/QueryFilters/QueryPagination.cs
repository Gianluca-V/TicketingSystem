using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketingSystem.Domain.QueryFilters
{
    public static class QueryPagination
    {
        public static IQueryable<T> ApplyPaging<T>(
            this IQueryable<T> query,
            int? page,
            int? take)
        {
            var currentPage = page ?? 1;
            var pageSize = take ?? 100;

            return query
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize);
        }
    }
}
