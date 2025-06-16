using Microsoft.AspNetCore.Mvc;
using QueuingSystemBe.Services;
using QueuingSystemBe.ViewModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

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
    [Authorize]
    public IActionResult GetByRole([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
            return Unauthorized(new { error = "Don't find email in token." });

        var result = _assignmentSvc.GetAssignmentsByRole(email, page, pageSize);
        return Ok(result);
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
    [HttpPut("to-processing")]
    [Authorize(Roles = "Doctor")]
    public IActionResult UpdateToProcessing([FromQuery] string code)
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
            return Unauthorized(new { error = "Không tìm thấy email trong token." });

        var result = _assignmentSvc.UpdateStatusToProcessing(code, email);
        return result ? Ok() : BadRequest("Không thể cập nhật trạng thái.");
    }

    [HttpPut("to-next")]
    [Authorize(Roles = "Doctor")]
    public IActionResult UpdateStatusToNext([FromQuery] string code)
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
            return Unauthorized(new { error = "Không tìm thấy email trong token." });

        var result = _assignmentSvc.UpdateStatusToNext(code, email);
        return result ? Ok() : BadRequest("Không thể cập nhật trạng thái.");
    }

    [HttpPut("sequence-update")]
    [Authorize(Roles = "Doctor")]
    public IActionResult UpdateStatusSequence([FromQuery] string code)
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
            return Unauthorized(new { error = "Không tìm thấy email trong token." });

        var result = _assignmentSvc.UpdateStatusSequence(code, email);
        return result ? Ok() : BadRequest("Không thể cập nhật trạng thái.");
    }

    [HttpGet("admin-filter")]
    [Authorize(Roles = "Admin")]
    public IActionResult FilterForAdmin([FromQuery] AssignmentFilterRequest request)
    {
        var result = _assignmentSvc.FilterAssignmentsForAdmin(request);
        return Ok(result);
    }
}

