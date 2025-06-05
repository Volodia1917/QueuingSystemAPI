namespace QueuingSystemBe.Dtos
{
    public class ServiceFilterDto
    {
        public bool? IsInOperation { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public string? Keyword { get; set; }
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 10;
    }
}
