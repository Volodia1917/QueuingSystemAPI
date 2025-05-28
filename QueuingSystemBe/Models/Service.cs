using System.ComponentModel.DataAnnotations;

namespace QueuingSystemBe.Models
{
    public class Service : UpdateInfor
    {
        [MaxLength(3)]
        public string ServiceCode { get; set; }
        [MaxLength(100)]
        public string ServiceName { get; set; }
        public string? Description { get; set; }
        public bool? IsInOperation { get; set; }
        public ICollection<Assignment>? Assignments { get; set; }
    }
}
