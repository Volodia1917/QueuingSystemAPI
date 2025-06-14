using Microsoft.AspNetCore.Mvc;
using QueuingSystemBe.Services;

[ApiController]
[Route("api/[controller]")]
public class AssignmentController : ControllerBase
{
    private readonly IAssignmentSvc _assignmentSvc;

    public AssignmentController(IAssignmentSvc assignmentSvc)
    {
        _assignmentSvc = assignmentSvc;
    }

    [HttpGet("byrole")]
    public IActionResult GetByRole([FromQuery] string email)
    {
        if (string.IsNullOrEmpty(email))
            return BadRequest(new { error = "Email is required" });

        var data = _assignmentSvc.GetAssignmentsByRole(email);
        return Ok(data);
    }

    [HttpPost("generate")]
    public IActionResult Generate([FromBody] AssignmentCreateRequest request)
    {
        try
        {
            var result = _assignmentSvc.GenerateNewAssignment(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
    [HttpPut("doctorupdatestatus")]
    public IActionResult UpdateStatus([FromQuery] string code, [FromQuery] string email)
    {
        if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(email))
            return BadRequest(new { error = "Code and email is required" });

        try
        {
            var updated = _assignmentSvc.UpdateStatusToProcessing(code, email);
            if (!updated)
                return NotFound(new { error = "Failed" });

            return Ok(new { message = "Update" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

}
