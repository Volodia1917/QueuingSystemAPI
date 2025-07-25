﻿using System.ComponentModel.DataAnnotations;

namespace QueuingSystemBe.ViewModels
{
    public class AddUserRequest
    {
        [MaxLength(60)]
        public string? Email { get; set; }
        [MaxLength(200)]
        public string? Password { get; set; }
        [MaxLength(100)]
        public string? FullName { get; set; }
        [MaxLength(20)]
        public string? Telephone { get; set; }
        [MaxLength(20)]
        public string? UserRole { get; set; }
        public string? Note { get; set; }
        public IFormFile? Avatar { get; set; }
        [MaxLength(3)]
        public string? ServiceCode { get; set; }
        public string? CreatedUser { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
    }
}
