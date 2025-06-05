using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QueuingSystemBe.Services;
using QueuingSystemBe.ViewModels;
using System.Security.Claims;

namespace QueuingSystemBe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceController : ControllerBase
    {
        private readonly IDeviceSvc _svc;

        public DeviceController(IDeviceSvc svc)
        {
            _svc = svc;
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetDevices([FromQuery] DeviceFilterRequest filter)
        {
            int totalRecords;
            var result = _svc.GetDevices(filter, out totalRecords);

            return Ok(new
            {
                data = result,
                total = totalRecords,
                page = filter.Page,
                limit = filter.Limit
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult AddDevice([FromForm] DeviceRequest request)
        {
            var currentEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            request.CreatedUser = currentEmail;
            request.CreatedDate = DateTimeOffset.UtcNow;

            var result = _svc.AddDevice(request);
            if (!result) return BadRequest("DeviceCode đã tồn tại.");
            return Ok("Thêm thiết bị thành công");
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{deviceCode}")]
        public IActionResult UpdateDevice(string deviceCode, [FromForm] DeviceRequest request)
        {
            var currentEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            request.UpdatedUser = currentEmail;
            request.UpdatedDate = DateTimeOffset.UtcNow;

            var result = _svc.UpdateDevice(deviceCode, request);
            if (!result) return NotFound("Không tìm thấy thiết bị.");
            return Ok("Cập nhật thành công");
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{deviceCode}")]
        public IActionResult DeleteDevice(string deviceCode)
        {
            var currentEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            var result = _svc.DeleteDevice(deviceCode, currentEmail ?? "Unknown");
            if (!result) return NotFound("Không tìm thấy thiết bị.");
            return Ok("Xóa thiết bị thành công");
        }
    }
}
