using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QueuingSystemBe.Services;
using QueuingSystemBe.ViewModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;


namespace QueuingSystemBe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private IUserSvc svc;
        public UserController(IUserSvc _svc)
        {
            this.svc = _svc;
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult AddUser([FromForm] UserRequest request)
        {
            string? currentEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            request.CreatedDate = request.CreatedDate?.ToOffset(TimeSpan.Zero);
            return Ok(svc.AddUser(currentEmail, request));
        }
        [Authorize]
        [HttpPut("{email}")]
        public IActionResult UpdateUser(string email, [FromForm] UserRequest request)
        {
            string? currentEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            request.UpdatedDate = request.UpdatedDate?.ToOffset(TimeSpan.Zero);
            return Ok(svc.UpdateUser(email, currentEmail, request));
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("{email}")]
        public IActionResult DeleteUser(string email, UserRequest request)
        {
            string? currentEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            request.DeletedDate = request.DeletedDate?.ToOffset(TimeSpan.Zero);
            return Ok(svc.DeleteUser(email, currentEmail, request));
        }
        [Authorize]
        [HttpGet("users")]
        public IActionResult GetUsers()
        {
            string? currentEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(currentEmail))
            {
                return Unauthorized("Current user not identified.");
            }

            return Ok(svc.GetUser(null,currentEmail));
        }

    }
}

