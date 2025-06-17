using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QueuingSystemBe.Helpers;
using QueuingSystemBe.Models;
using QueuingSystemBe.Services;
using QueuingSystemBe.ViewModels;

namespace QueuingSystemBe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationSvc _authService;
        private readonly MyDbContext _myDbContext;

        public AuthenticationController(IAuthenticationSvc authService, MyDbContext myDbContext)
        {
            _authService = authService;
            _myDbContext = myDbContext;
        }

        /// <summary>
        /// Đăng nhập và nhận Access Token + Refresh Token
        /// </summary>
        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLoginRequest request)
        {
            var tokenResponse = _authService.GetToken(request);
            if (string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                return Unauthorized("Invalid credentials");
            }

            var userInfo = _authService.GetUserByLogin(request);
            return Ok(new 
            {
                Token = tokenResponse,
                User = userInfo
            });
        }

        /// <summary>
        /// Cấp lại Access Token nếu Refresh Token còn hạn
        /// </summary>
        [HttpPost("refresh-token")]
        public IActionResult RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var newAccessToken = _authService.RefreshAccessToken(request.Email, request.RefreshToken!);
            if (string.IsNullOrEmpty(newAccessToken.RefreshToken) || string.IsNullOrEmpty(newAccessToken.AccessToken))
            {
                return Unauthorized("Invalid credentials");
            }

            return Ok(new
            {
                AccessToken = newAccessToken.AccessToken,
                RefreshToken = newAccessToken.RefreshToken,
                ExpiredAccessToken = newAccessToken.ExpiredAccessToken,
                ExpiredRefreshToken = newAccessToken.ExpiredRefreshToken,
            });
        }
        [Authorize]
        [HttpDelete("logout/{email}")]
        public IActionResult Logout(string email)
        {
            bool result = _authService.Logout(email);
            if (result)
                return Ok(new { message = "Logout successful. Refresh tokens removed." });

            return NotFound(new { message = "User not found or no tokens to remove." });
        }
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var user = _myDbContext.Users.FirstOrDefault(u => u.Email == request.Email);
            if (user == null) return Ok(new { message = "Email này chưa được đăng kí" });
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            await _authService.SendResetPasswordEmailAsync(request.Email, baseUrl);
            return Ok(new { message = "Nếu email tồn tại, bạn sẽ nhận được liên kết đặt lại mật khẩu." });
        }


        [HttpGet("reset-password")]
        public async Task<IActionResult> ResetPassword([FromQuery] string token)
        {
            var success = await _authService.ResetPasswordAsync(token);
            if (success)
                return Ok("Mật khẩu mới đã được gửi đến email của bạn.");
            else
                return BadRequest("Token không hợp lệ hoặc đã hết hạn.");
        }


    }
}
