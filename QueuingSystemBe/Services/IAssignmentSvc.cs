using QueuingSystemBe.Models;
using QueuingSystemBe.ViewModels;
public interface IAssignmentSvc
{
    PagedResult<Assignment> GetAssignmentsByRole(string email, int page = 1, int pageSize = 10);
    Assignment GenerateNewAssignment(AssignmentCreateRequest request);
    bool UpdateStatusToProcessing(string code, string email);
    bool UpdateStatusToNext(string code, string email);
    bool UpdateStatusSequence(string code, string email);
    public PagedResult<Assignment> FilterAssignmentsForAdmin(AssignmentFilterRequest request);
}

