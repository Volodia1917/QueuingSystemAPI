namespace QueuingSystemBe.ViewModels
{
    public class RefreshTokenRequest
    {
        public string Email { get; set; }
        public string? RefreshToken { get; set; }
        public DateTimeOffset? RefreshTokenExpiryTime { get; set; }
    }
}
