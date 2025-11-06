namespace LogiTrack.Models
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasNextPage => Page < TotalPages;
        public bool HasPreviousPage => Page > 1;
    }

    public class PerformanceMetrics
    {
        public string Endpoint { get; set; } = string.Empty;
        public long ResponseTimeMs { get; set; }
        public string CacheStatus { get; set; } = "N/A";
        public int QueryCount { get; set; }
        public DateTime Timestamp { get; set; }
    }
}