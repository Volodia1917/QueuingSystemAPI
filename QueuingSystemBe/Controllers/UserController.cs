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
        public IActionResult AddUser([FromForm] AddUserRequest addUserRequest)
        {
            string? currentEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            addUserRequest.CreatedDate = addUserRequest.CreatedDate?.ToOffset(TimeSpan.Zero);
            return Ok(svc.AddUser(currentEmail, addUserRequest));
        }
        [Authorize]
        [HttpPut("{email}")]
        public IActionResult UpdateUser(string email, [FromForm] UpdateUserRequest updateUserRequest)
        {
            string? currentEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            updateUserRequest.UpdatedDate = updateUserRequest.UpdatedDate?.ToOffset(TimeSpan.Zero);
            return Ok(svc.UpdateUser(email, currentEmail, updateUserRequest));
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

