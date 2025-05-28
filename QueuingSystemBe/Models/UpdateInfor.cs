using System;

namespace QueuingSystemBe.Models
{
    public class UpdateInfor
    {
        public DateTimeOffset? CreatedDate { get; set; }
        public DateTimeOffset? UpdatedDate { get; set; }
        public DateTimeOffset? DeletedDate { get; set; }
        public string? CreatedUser { get; set; }
        public string? UpdatedUser { get; set; }
        public string? DeletedUser { get; set; }
    }
}
