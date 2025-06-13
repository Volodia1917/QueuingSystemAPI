using System.ComponentModel.DataAnnotations;

namespace QueuingSystemBe.ViewModels
{
    public class ServiceResponse
    {
        [MaxLength(3)]
        public string ServiceCode { get; set; }
        [MaxLength(100)]
        public string ServiceName { get; set; }
        public string? Description { get; set; }
        public bool? IsInOperation { get; set; } = true;
        public DateTimeOffset? CreatedDate { get; set; }
    }
}
