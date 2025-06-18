using System.ComponentModel.DataAnnotations;

namespace QueuingSystemBe.ViewModels
{
    public class UpdateUserRequest
    {
        [MaxLength(100)]
        public string? FullName { get; set; }
         [MaxLength(200)]
        public string? Password { get; set; }
        [MaxLength(20)]
        public string? Telephone { get; set; }
        [MaxLength(20)]
        public string? Note { get; set; }
        [MaxLength(20)]
        public string? UserRole { get; set; }
        public IFormFile? Avatar { get; set; }
        [MaxLength(3)]
        public string? ServiceCode { get; set; }
        public string? UpdatedUser { get; set; }
        public DateTimeOffset? UpdatedDate { get; set; }
    }
}
