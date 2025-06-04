using QueuingSystemBe.Models;
public interface IAssignmentSvc
{
    List<Assignment> GetAll();
    List<Assignment> GetAssignmentsByRole(string email);
    Assignment GenerateNewAssignment(AssignmentCreateRequest request);

}
public class AssignmentCreateRequest
{
    public string CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string Telephone { get; set; }
    public string ServiceCode { get; set; }
    public string DeviceCode { get; set; }
}
