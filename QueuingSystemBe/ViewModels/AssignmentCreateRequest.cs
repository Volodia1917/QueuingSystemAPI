namespace QueuingSystemBe.ViewModels
{
    public class AssignmentCreateRequest
    {
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public string? Telephone { get; set; }
        public string ServiceCode { get; set; }
        public string DeviceCode { get; set; }
    }
}
