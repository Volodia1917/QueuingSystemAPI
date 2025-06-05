namespace QueuingSystemBe.ViewModels
{
    public class DeviceFilterRequest
    {
        public string? Keyword { get; set; }
        public bool? Connected { get; set; }
        public bool? OperationStatus { get; set; }
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 10;
    }
}
