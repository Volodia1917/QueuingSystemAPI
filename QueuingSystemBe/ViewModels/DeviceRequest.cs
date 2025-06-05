using System;

namespace QueuingSystemBe.ViewModels
{
    public class DeviceRequest
    {
        public string DeviceCode { get; set; }
        public string DeviceName { get; set; }
        public string IpAddress { get; set; }
        public bool? Connected { get; set; }
        public bool? OperationStatus { get; set; }

        public DateTimeOffset? CreatedDate { get; set; }
        public DateTimeOffset? UpdatedDate { get; set; }

        // ✅ Thêm các thuộc tính này
        public string? CreatedUser { get; set; }
        public string? UpdatedUser { get; set; }
    }
}
