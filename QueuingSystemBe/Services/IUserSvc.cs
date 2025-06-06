using QueuingSystemBe.Models;
using QueuingSystemBe.ViewModels;

namespace QueuingSystemBe.Services
{
    public interface IUserSvc
    {
        public string AddUser(string currentUserEmail, UserRequest request);
        public List<UserResponse> GetUser(string? email, string currentUserEmail);
        public string UpdateUser(string email,string currentUserEmail, UserRequest request);
        public string DeleteUser(string email, string currentUserEmail, DeleteUserRequest delete);
    }
}
