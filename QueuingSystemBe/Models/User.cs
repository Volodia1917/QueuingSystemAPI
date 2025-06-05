using System.ComponentModel.DataAnnotations;

namespace QueuingSystemBe.Models
{
    public class User : UpdateInfor
    {
        [Key]
        [MaxLength(60)]
        public string Email { get; set; }

        [MaxLength(200)]
        public string Password { get; set; }

        [MaxLength(100)]
        public string FullName { get; set; }

        [MaxLength(20)]
        public string? Telephone { get; set; }

        [MaxLength(20)]
        public string? UserRole { get; set; }

        public string? Note { get; set; }

        public byte[]? Avatar { get; set; }

        public bool IsDeleted { get; set; } = false; // ✅ Thêm dòng này để fix lỗi
    }
}
