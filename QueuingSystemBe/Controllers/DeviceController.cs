using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QueuingSystemBe.Services;
using QueuingSystemBe.ViewModels;

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
            var result = _svc.GetDevices(filter, out int total);
            return Ok(new { data = result, total, page = filter.Page, limit = filter.Limit });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult AddDevice([FromBody] DeviceRequest request)
        {
            request.CreatedUser = User.Identity?.Name ?? "System";
            request.CreatedDate = DateTimeOffset.UtcNow;
            var success = _svc.AddDevice(request);
            return Ok(new { success });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{deviceCode}")]
        public IActionResult UpdateDevice(string deviceCode, [FromBody] DeviceRequest request)
        {
            request.UpdatedUser = User.Identity?.Name ?? "System";
            request.UpdatedDate = DateTimeOffset.UtcNow;
            var success = _svc.UpdateDevice(deviceCode, request);
            return Ok(new { success });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{deviceCode}")]
        public IActionResult DeleteDevice(string deviceCode)
        {
            var deletedBy = User.Identity?.Name ?? "System";
            var success = _svc.DeleteDevice(deviceCode, deletedBy);
            return Ok(new { success });
        }
    }
}
