namespace QueuingSystemBe.ViewModels
{
    public class TokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string ExpiredAccessToken { get; set; } = string.Empty;
        public string ExpiredRefreshToken { get; set; } = string.Empty;
    }
}
