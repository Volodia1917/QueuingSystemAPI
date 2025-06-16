namespace QueuingSystemBe.ViewModels
{
    public class AssignmentFilterRequest
    {
        public string? ServiceCode { get; set; }
        public int? Status { get; set; }
        public string? Source { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public string? Keyword { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
