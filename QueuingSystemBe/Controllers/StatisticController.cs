using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QueuingSystemBe.Services;
using Microsoft.AspNetCore.Authorization;
using System.Reflection.Metadata.Ecma335;


namespace QueuingSystemBe.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticController : ControllerBase
    {
        private IStatisticSvc svc;
        public StatisticController(IStatisticSvc svc)
        {
            this.svc = svc;
        }     
        [HttpGet]
        public IActionResult GetAssignments()
        {
            return Ok(svc.GetAssignments());
        }
        [HttpGet("used")]
        public IActionResult GetAssignmentUsed()
        {
            return Ok(svc.GetAssignmentUsed());
        }
        [HttpGet("wait")]
        public IActionResult GetAssignmentWait()
        {
            return Ok(svc.GetAssignmentWait());
        }
        [HttpGet("skip")]
        public IActionResult GetAssignmentSkip()
        {
            return Ok(svc.GetAssignmentSkip());
        }
        [HttpGet("device")]
        public IActionResult GetDevice()
        {
            return Ok(svc.GetDevice());
        }
        [HttpGet("service")]
        public IActionResult GetService()
        {
            return Ok(svc.GetService());
        }
    }
}

