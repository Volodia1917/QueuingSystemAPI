using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QueuingSystemBe.Helpers;
using QueuingSystemBe.Models;
using QueuingSystemBe.ViewModels;

namespace QueuingSystemBe.Services
{
    public class AuthenticationSvc : IAuthenticationSvc
    {
        private readonly MyDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthenticationSvc(MyDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
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

            string imageData = user.Avatar != null ? Convert.ToBase64String(user.Avatar) : string.Empty;

            return new UserLoginResponse
            {
                Email = user.Email,
                FullName = user.FullName,
                AvatarUrl = imageData
            };
        }

        // Tạo access token và refresh token
        public TokenResponse GetToken(UserLoginRequest request)
        {
            if (!ConfirmLogin(request)) return new TokenResponse();

            var user = _context.Users.First(u => u.Email == request.Email);
            string accessToken = GenerateAccessToken(user.Email, user.UserRole ?? "user");
            string refreshToken = GenerateRefreshToken();

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
                RefreshToken = refreshToken
            };
        }

        // Cấp lại access token nếu refresh token hợp lệ
        public TokenResponse? RefreshAccessToken(string email, string refreshToken)
        {
            if (!ValidateRefreshToken(email, refreshToken)) return new TokenResponse();

            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null) return new TokenResponse();

            string newAccessToken = GenerateAccessToken(user.Email, user.UserRole ?? "user");
            string newRefreshToken = GenerateRefreshToken();

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
                RefreshToken = newRefreshToken
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
                new Claim(ClaimTypes.Role, role ?? "user")
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
