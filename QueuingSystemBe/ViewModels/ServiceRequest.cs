using System.ComponentModel.DataAnnotations;

namespace QueuingSystemBe.ViewModels
{
    public class ServiceRequest
    {
        [MaxLength(100)]
        public string ServiceName { get; set; }
        public string? Description { get; set; }
        public bool? IsInOperation { get; set; }
    }
}
