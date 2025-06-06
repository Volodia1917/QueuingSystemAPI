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
            return Ok(new { total = svc.GetAssignments() });
        }

        [HttpGet("used")]
        public IActionResult GetAssignmentUsed()
        {
            return Ok(new { total = svc.GetAssignmentUsed() });
        }

        [HttpGet("wait")]
        public IActionResult GetAssignmentWait()
        {
            return Ok(new { total = svc.GetAssignmentWait() });
        }

        [HttpGet("skip")]
        public IActionResult GetAssignmentSkip()
        {
            return Ok(new { total = svc.GetAssignmentSkip() });
        }

        [HttpGet("day")]
        public IActionResult GetStatisticByMonth(int month)
        {
            return Ok(new { total = svc.GetStatisticByMonth(month) });
        }


        [HttpGet("week")]
        public IActionResult GetStatisticByWeek(int month)
        {
            return Ok(new { total = svc.GetStatisticByWeek(month) });
        }


        [HttpGet("year")]
        public IActionResult GetStatisticByYear()
        {
            return Ok(new { total = svc.GetStatisticByYear() });
        }

        [HttpGet("device")]
        public IActionResult GetDevice()
        {
            return Ok(new { total = svc.GetDevice() });
        }
        [HttpGet("service")]
        public IActionResult GetService()
        {
            return Ok(new { total = svc.GetService() });
        }
    }
}

