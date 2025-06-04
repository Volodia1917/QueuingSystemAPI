using System.ComponentModel.DataAnnotations;

namespace QueuingSystemBe.Models
{
    public class Assignment : UpdateInfor
    {
        [MaxLength(10)]
        public string? Code { get; set; }
        [MaxLength(100)]
        public string CustomerName { get; set; }
        [MaxLength(80)]
        public string? CustomerEmail { get; set; }
        [MaxLength(20)]
        public string Telephone { get; set; }
        public DateTimeOffset AssignmentDate { get; set; }
        public DateTimeOffset ExpireDate { get; set; }
        public byte? Status { get; set; }
        [MaxLength(3)]
        public string ServiceCode { get; set; }
        [MaxLength(20)]
        public string DeviceCode { get; set; }
        public Service? Service { get; set; }
        public Device? Device { get; set; }
    }
}
