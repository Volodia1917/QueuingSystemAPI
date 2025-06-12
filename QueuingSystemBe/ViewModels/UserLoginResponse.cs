namespace QueuingSystemBe.ViewModels
{
    public class UserLoginResponse
    {
        public string Email { get; set; }
        public string FullName { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Role {  get; set; }
    }
}
