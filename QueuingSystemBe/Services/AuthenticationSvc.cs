using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using QueuingSystemBe.Helpers;
using QueuingSystemBe.HubForRealTime;
using QueuingSystemBe.Models;
using QueuingSystemBe.ViewModels;

namespace QueuingSystemBe.Services
{
    public class AuthenticationSvc : IAuthenticationSvc
    {
        private readonly MyDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHubContext<AuthHub> _hubContext;
        private readonly UserConnectSvc _connectionManager;

        public AuthenticationSvc(
            MyDbContext context,
            IConfiguration configuration,
            IHubContext<AuthHub> hubContext,
            UserConnectSvc connectionManager
            ){
            _context = context;
            _configuration = configuration;
            _hubContext = hubContext;
            _connectionManager = connectionManager;
        }

        private static List<RefreshTokenRequest> RefreshTokens = new();

        // Xác thực thông tin đăng nhập
        public bool ConfirmLogin(UserLoginRequest request)
        {
            string hashedPassword = EncryptMd5.MD5Function(request.Password);
            return _context.Users.Any(u => u.Email == request.Email && u.Password == hashedPassword);
        }

        // Trả thông tin user sau khi đăng nhập thành công
        public UserLoginResponse? GetUserByLogin(UserLoginRequest request)
        {
            string hashedPassword = EncryptMd5.MD5Function(request.Password);
            var user = _context.Users.FirstOrDefault(u => u.Email == request.Email && u.Password == hashedPassword);
            if (user == null) return null;

            string imageBase64Data = user.Avatar != null ? Convert.ToBase64String(user.Avatar) : string.Empty;
            string imageData = (imageBase64Data != "") ? string.Format("data:image/jpg;base64,{0}", imageBase64Data) : "";
            string role = user.UserRole ?? "Staff";

            return new UserLoginResponse
            {
                Email = user.Email,
                FullName = user.FullName,
                AvatarUrl = imageData,
                Role = role,
            };
        }

        // Tạo access token và refresh token
        public TokenResponse GetToken(UserLoginRequest request)
        {
            if (!ConfirmLogin(request)) return new TokenResponse();

            var user = _context.Users.First(u => u.Email == request.Email);

            //xóa connectionId cũ
            if (_connectionManager.TryGetConnection(user.Email, out var oldConnectionId))
            {
                _hubContext.Clients.Client(oldConnectionId).SendAsync("ForceLogout");
                _connectionManager.RemoveConnection(user.Email); // xóa kết nối cũ
            }
          
            string accessToken = GenerateAccessToken(user.Email, user.UserRole ?? "Staff");
            string newExpiredAccessToken = "";
            if (!string.IsNullOrEmpty(accessToken)) newExpiredAccessToken = DateTimeOffset.UtcNow.AddHours(1).ToString("yyyy-MM-dd HH:mm:ss");
            string refreshToken = GenerateRefreshToken();
            string newExpiredRefreshToken = "";
            if (!string.IsNullOrEmpty(accessToken)) newExpiredRefreshToken = DateTimeOffset.UtcNow.AddDays(7).ToString("yyyy-MM-dd HH:mm:ss");

            RefreshTokens.RemoveAll(r => r.Email == user.Email);
            RefreshTokens.Add(new RefreshTokenRequest
            {
                Email = user.Email,
                RefreshToken = refreshToken,
                RefreshTokenExpiryTime = DateTimeOffset.UtcNow.AddHours(2)
            });

            return new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiredAccessToken = newExpiredAccessToken,
                ExpiredRefreshToken = newExpiredRefreshToken,
            };
        }

        // Cấp lại access token nếu refresh token hợp lệ
        public TokenResponse? RefreshAccessToken(string email, string refreshToken)
        {
            if (!ValidateRefreshToken(email, refreshToken)) return new TokenResponse();

            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null) return new TokenResponse();

            string newAccessToken = GenerateAccessToken(user.Email, user.UserRole ?? "Staff");
            string newExpiredAccessToken = "";
            if (!string.IsNullOrEmpty(newAccessToken)) newExpiredAccessToken = DateTimeOffset.UtcNow.AddHours(1).ToString("yyyy-MM-dd HH:mm:ss");
            string newRefreshToken = GenerateRefreshToken();
            string newExpiredRefreshToken = "";
            if (!string.IsNullOrEmpty(newRefreshToken)) newExpiredRefreshToken = DateTimeOffset.UtcNow.AddDays(7).ToString("yyyy-MM-dd HH:mm:ss");

            RefreshTokens.RemoveAll(r => r.Email == user.Email);
            RefreshTokens.Add(new RefreshTokenRequest
            {
                Email = user.Email,
                RefreshToken = newRefreshToken,
                RefreshTokenExpiryTime = DateTimeOffset.UtcNow.AddDays(7)
            });

            return new TokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiredAccessToken = newExpiredAccessToken,
                ExpiredRefreshToken = newExpiredRefreshToken,
            };
        }

        // Tạo access token từ email và role
        private string GenerateAccessToken(string email, string role)
        {
            var secretKey = _configuration["Jwt:Key"];
            var securityKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secretKey));
            var signIn = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(ClaimTypes.Email, email),
                new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
                new Claim(ClaimTypes.Role, role ?? "Staff")
                };
            var tokens = new JwtSecurityToken(_configuration["Jwt:Issuer"], _configuration["Jwt:Audience"],
                claims, null, DateTime.Now.AddHours(1), signIn);
            string myTokens = new JwtSecurityTokenHandler().WriteToken(tokens);

            return myTokens;
        }

        // Tạo refresh token ngẫu nhiên
        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        // Kiểm tra refresh token hợp lệ
        public bool ValidateRefreshToken(string email, string refreshToken)
        {
            var token = RefreshTokens.FirstOrDefault(t => t.Email == email && t.RefreshToken == refreshToken);
            if (token == null) return false;
            return token.RefreshTokenExpiryTime >= DateTimeOffset.UtcNow;
        }
        //Đăng xuất, xóa refresh token
        public bool Logout(string email)
        {
            var tokensToRemove = RefreshTokens.RemoveAll(r => r.Email == email);
            return tokensToRemove > 0;
        }

    }
}
