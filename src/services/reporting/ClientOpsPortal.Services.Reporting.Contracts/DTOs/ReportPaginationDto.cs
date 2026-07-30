using System;
using System.Collections.Generic;
using System.Text;

namespace ClientOpsPortal.Services.Reporting.Contracts.DTOs
{
    public class ReportPaginationDto<T> where T : class
    {
        public required IEnumerable<T> Items { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
        public bool HasNext => Page < TotalPages;
        public bool HasPrevious => Page > 1;

        public static ReportPaginationDto<T> Create(IEnumerable<T> items, int page, int pageSize, int totalCount) =>
            new()
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
    }
}
