using System.ComponentModel.DataAnnotations;

namespace QueuingSystemBe.Models
{
    public class Device : UpdateInfor
    {
        [MaxLength(20)]
        public string DeviceCode { get; set; }
        [MaxLength(100)]
        public string DeviceName { get; set; }
        [MaxLength(20)]
        public string IpAddress { get; set; }
        public bool? Connected { get; set; }
        public bool? OperationStatus { get; set; }
        public ICollection<Assignment>? Assignments { get; set; }
    }
}
