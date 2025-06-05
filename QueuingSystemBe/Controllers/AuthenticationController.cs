using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QueuingSystemBe.Services;
using QueuingSystemBe.ViewModels;

namespace QueuingSystemBe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationSvc _authService;

        public AuthenticationController(IAuthenticationSvc authService)
        {
            _authService = authService;
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
                RefreshToken = newAccessToken.RefreshToken
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
    }
}
