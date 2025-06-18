namespace QueuingSystemBe.ViewModels
{
    public class DeviceResponse
    {
        public string DeviceCode { get; set; }
        public string DeviceName { get; set; }
        public string IpAddress { get; set; }
        public bool? Connected { get; set; }
        public bool? OperationStatus { get; set; }
    }
}
