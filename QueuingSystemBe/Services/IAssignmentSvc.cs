using QueuingSystemBe.Models;
using QueuingSystemBe.ViewModels;
public interface IAssignmentSvc
{
    object GetAssignmentsByRole(string email);
    Assignment GenerateNewAssignment(AssignmentCreateRequest request);
    bool UpdateStatusToProcessing(string code, string email);
    bool UpdateStatusToNext(string code, string email);
    bool UpdateStatusSequence(string code, string email);
    public object FilterAssignmentsForAdmin(AssignmentFilterRequest request);
}

