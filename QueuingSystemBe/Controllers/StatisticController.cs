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

        [HttpGet("dashboard")]
        public IActionResult GetDashboardOverview(
            [FromQuery] string period = "day",
            [FromQuery] int? month = null)
        {
            try
            {
                var dashboard = svc.GetDashboardOverview(period, month);
                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error retrieving dashboard data", error = ex.Message });
            }
        }

        [HttpGet("numbers-overview")]
        public IActionResult GetNumbersOverview()
        {
            try
            {
                var overview = svc.GetNumbersOverview();
                return Ok(overview);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error retrieving numbers overview", error = ex.Message });
            }
        }

        [HttpGet("chart-data")]
        public IActionResult GetChartData(
     [FromQuery] string period = "day",
     [FromQuery] int? month = null) 
        {
            try
            {
                var chartData = svc.GetChartData(period, month);
                return Ok(chartData);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error retrieving chart data", error = ex.Message });
            }
        }

        [HttpGet("overall-summary")]
        public IActionResult GetOverallSummary()
        {
            try
            {
                var summary = svc.GetOverallSummary();
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error retrieving overall summary", error = ex.Message });
            }
        }
    }
}

