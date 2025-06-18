using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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
        private readonly EmailSettings _emailSettings;

        public AuthenticationSvc(
            MyDbContext context,
            IConfiguration configuration,
            IHubContext<AuthHub> hubContext,
            UserConnectSvc connectionManager,
            IOptions<EmailSettings> emailSettings
            ){
            _context = context;
            _configuration = configuration;
            _hubContext = hubContext;
            _connectionManager = connectionManager;
            _emailSettings = emailSettings.Value;
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
          
            string accessToken = GenerateAccessToken(user.Email, user.UserRole ?? "Staff", 60);
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

            string newAccessToken = GenerateAccessToken(user.Email, user.UserRole ?? "Staff", 60);
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
        private string GenerateAccessToken(string email, string role, int addMinutes)
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
                claims, null, DateTime.Now.AddMinutes(addMinutes), signIn);
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
        public async Task SendResetPasswordEmailAsync(string email, string baseUrl)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null) return;

            var token = GenerateAccessToken(email, user.UserRole ?? "Staff", 5);
            var resetLink = $"{baseUrl}/api/authentication/reset-password?token={token}";

            var mail = new MailMessage
            {
                From = new MailAddress(_emailSettings.SenderEmail, "Queuing System"),
                Subject = "Yêu cầu đặt lại mật khẩu từ hệ thống Queuing System",
                IsBodyHtml = true,
                Body = $@"
            <html>
            <body style='font-family: Arial, sans-serif; font-size: 14px; color: #333;'>
                <h2>Xin chào <strong>{user.FullName}</strong>,</h2>

                <p>Chúng tôi đã nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn <strong>{email}</strong> trên hệ thống <em>Queuing System</em>.</p>

                <p>Vui lòng nhấp vào liên kết bên dưới để tiến hành đặt lại mật khẩu:</p>

                <p>
                    <a href='{resetLink}' style='color: #1a73e8; text-decoration: none;'>{resetLink}</a>
                </p>

                <p style='color: #888888; font-size: 13px;'>
                    Lưu ý: Liên kết sẽ hết hạn sau một thời gian ngắn vì lý do bảo mật. Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email này.
                    <span style='color:transparent;'>.</span>
                </p>

                <p>Trân trọng,<span style='color:transparent;'>.</span></p>
                <p><strong>Đội ngũ hỗ trợ Queuing System</strong><span style='color:transparent;'>.</span></p>
            </body>
            </html>"
            };

            mail.To.Add(email);

            using var smtp = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port)
            {
                Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.SenderPassword),
                EnableSsl = true
            };

            await smtp.SendMailAsync(mail);
        }

        public async Task<bool> ResetPasswordAsync(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var email = principal.FindFirst(ClaimTypes.Email)?.Value;
                var user = _context.Users.FirstOrDefault(u => u.Email == email);
                if (user == null) return false;

                var newPassword = GenerateRandomPassword();
                user.Password = EncryptMd5.MD5Function(newPassword);
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                await SendNewPasswordEmail(email, newPassword);
                return true;
            }
            catch
            {
                return false;
            }
        }
        private async Task SendNewPasswordEmail(string email, string newPassword)
        {
            var mail = new MailMessage
            {
                From = new MailAddress(_emailSettings.SenderEmail, "Queuing System"),
                Subject = "Mật khẩu mới từ hệ thống Queuing System",
                IsBodyHtml = true,
                Body = $@"
        <html>
        <body style='font-family: Arial, sans-serif; font-size: 14px; color: #333;'>
            <p>Xin chào <strong>{email}</strong>,</p>

            <p>Hệ thống đã tiến hành đặt lại mật khẩu cho tài khoản của bạn theo yêu cầu.</p>

            <p><strong>Mật khẩu mới:</strong> <span style='font-family: monospace;'>{newPassword}</span></p>

            <p>Vui lòng sử dụng mật khẩu này để đăng nhập và thay đổi lại mật khẩu mới trong phần Cài đặt tài khoản để đảm bảo bảo mật thông tin.</p>

            <p>Trân trọng,</p>
            <p><strong>Đội ngũ hỗ trợ Queuing System</strong></p>
        </body>
        </html>"
            };
            mail.To.Add(email);


            using var smtp = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port)
            {
                Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.SenderPassword),
                EnableSsl = true
            };

            await smtp.SendMailAsync(mail);
        }
        private string GenerateRandomPassword(int length = 8)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder result = new();
            using var rng = new RNGCryptoServiceProvider();
            byte[] buffer = new byte[sizeof(uint)];

            while (length-- > 0)
            {
                rng.GetBytes(buffer);
                uint num = BitConverter.ToUInt32(buffer, 0);
                result.Append(chars[(int)(num % (uint)chars.Length)]);
            }

            return result.ToString();
        }

    }
}
