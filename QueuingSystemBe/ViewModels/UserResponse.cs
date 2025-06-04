using QueuingSystemBe.Models;
using System.ComponentModel.DataAnnotations;

namespace QueuingSystemBe.ViewModels
{
    public class UserResponse : UpdateInfor
    {
        [MaxLength(60)]
        public string Email { get; set; }

        [MaxLength(100)]
        public string FullName { get; set; }
        [MaxLength(20)]
        public string? Telephone { get; set; }
        [MaxLength(20)]
        public string? UserRole { get; set; }
        public string? Note { get; set; }
        public string ImageUrl { get; set; }
    }
}
