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

    [HttpGet]
    public IActionResult GetAll()
    {
        var data = _assignmentSvc.GetAll();
        return Ok(data);
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
}
