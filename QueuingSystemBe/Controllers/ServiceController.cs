using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QueuingSystemBe.Dtos;
using QueuingSystemBe.Models;
using QueuingSystemBe.Services;
using QueuingSystemBe.ViewModels;
using System.Text.Json;

namespace QueuingSystemBe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        private IServiceSvc _svc;
        public ServiceController(IServiceSvc _svc)
        {
            this._svc = _svc;
        }
        [AllowAnonymous]
        [HttpGet("all")]
        public IActionResult GetService1()
        {
            return Ok(_svc.GetServices1());
        }
       


        [AllowAnonymous]
        [HttpGet]
        public IActionResult GetService([FromQuery] ServiceFilterDto filter)
        {
            int totalRecords;
            var services = _svc.GetServices(filter, out totalRecords);

            return Ok(new
            {
                data = services,
                total = totalRecords,
                page = filter.Page,
                limit = filter.Limit
            });
        }



        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult AddNewService([FromForm] ServiceRequest request)
        {
            return Ok(_svc.AddService(request));
        }



        [Authorize(Roles = "Admin")]
        [HttpDelete("{ServiceCode}")]       
        public IActionResult DeleteService([FromRoute] string ServiceCode)
        {
            return Ok(_svc.DeleteService(ServiceCode));
        }



        [Authorize(Roles = "Admin")]
        [HttpPut("{ServiceCode}")]
        public IActionResult UpdateService(string ServiceCode, [FromForm] ServiceRequest request)
        {
            return Ok(_svc.UpdateService(ServiceCode, request));
        }


    }
}