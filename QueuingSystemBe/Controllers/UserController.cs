using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QueuingSystemBe.Services;
using QueuingSystemBe.ViewModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using QueuingSystemBe.Models;


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
            return Ok(svc.AddUser("currentEmail", request));
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
        public IActionResult DeleteUser(string email, [FromQuery] DeleteUserRequest delete)
        {
            string? currentEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            delete.DeletedDate = delete.DeletedDate?.ToOffset(TimeSpan.Zero);
            return Ok(svc?.DeleteUser(email, currentEmail, delete));

        }
        [Authorize]
        [HttpGet("users")]
        public IActionResult GetUsers([FromQuery] string? email)
        {
            string? currentEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(currentEmail))
            {
                return Unauthorized("Current user not identified.");
            }

            return Ok(svc.GetUser(email, currentEmail));
        }
    }
}

