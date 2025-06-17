using QueuingSystemBe.ViewModels;

namespace QueuingSystemBe.Services
{
    public interface IAuthenticationSvc
    {
        public bool ConfirmLogin(UserLoginRequest request);
        public UserLoginResponse? GetUserByLogin(UserLoginRequest request);
        public TokenResponse GetToken(UserLoginRequest request);
        public TokenResponse? RefreshAccessToken(string email, string refreshToken);
        public bool ValidateRefreshToken(string email, string refreshToken);
        public bool Logout(string email);
        Task SendResetPasswordEmailAsync(string email, string baseUrl);
        Task<bool> ResetPasswordAsync(string token);
    }
}
