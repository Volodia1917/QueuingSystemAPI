using QueuingSystemBe.Models;
using QueuingSystemBe.ViewModels;
using QueuingSystemBe.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Identity;

namespace QueuingSystemBe.Services
{
    public class UserSvc : IUserSvc
    {
        private MyDbContext _dbcontext ;
        private IWebHostEnvironment environment ;
        public UserSvc(MyDbContext dbcontext, IWebHostEnvironment environment)
        {
            _dbcontext = dbcontext;
            this.environment = environment;
        }

        public string AddUser(string currentUserEmail, UserRequest request)
        {
            try
            {
                User user = new User();
                user.Email = request.Email;
                user.FullName = request.FullName;
                user.Password = EncryptMd5.MD5Function(request.Password);
                user.Telephone = request.Telephone;
                user.UserRole = request.UserRole;
                user.Note = request.Note;
                user.CreatedDate = request.CreatedDate;
                user.ServiceCode = request.ServiceCode;
               // user.IsDeleted = request.IsDeleted;
                user.CreatedUser = currentUserEmail;
                if (request.Avatar != null) {
                    MemoryStream stream = new MemoryStream();
                    request.Avatar.CopyTo(stream);
                    user.Avatar = stream.ToArray();
                }
                _dbcontext.Users.Add(user);
                _dbcontext.SaveChanges();
                return "Added";

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public string DeleteUser(string email, string currentUserEmail,DeleteUserRequest delete)
        {

            User user = _dbcontext.Users.Where(x => x.Email == email).FirstOrDefault();
            

            try
            {
                if (user != null)
                {
                    user.DeletedUser = currentUserEmail;
                    user.DeletedDate = delete.DeletedDate;
                    string logContent = $"Email: {user.Email}\n" +
                                $"FullName:{user.FullName}\n" +
                                $"Telephone: {user.Telephone}\n" +
                                $"Note: {user.Note}\n" +
                                $"UserRole: {user.UserRole}\n" +
                                $"DeletedBy: {user.DeletedUser}\n" +
                                $"DeletedDate: {user.DeletedDate}\n" +
                                $"--------------------------\n";

                    string deletedFolder = Path.Combine(environment.WebRootPath, "Deleted");
                    if (!Directory.Exists(deletedFolder))
                    {
                        Directory.CreateDirectory(deletedFolder);
                    }

                    string fullPath = Path.Combine(deletedFolder, "DeletedUsers.txt");
                    File.AppendAllText(fullPath, logContent);
                    _dbcontext.Users.Remove(user);
                    _dbcontext.SaveChanges();
                    return "Deleted";
                }
                return "Not Found";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }



        public List<UserResponse> GetUser(string? email, string currentUserEmail)
        {
            // Lấy user hiện tại theo currentUserEmail để check role
            User user = _dbcontext.Users.FirstOrDefault(u => u.Email == currentUserEmail);

            if (user == null)
                return new List<UserResponse>();

            List<User> users;

            if (string.Equals(user.UserRole, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                // Admin: nếu có email truyền vào thì lọc theo email, còn không thì lấy tất cả
                if (!string.IsNullOrEmpty(email))
                {
                    users = _dbcontext.Users
                        .Where(u => u.Email.Contains(email))
                        .ToList();
                }
                else
                {
                    users = _dbcontext.Users.ToList();
                }
            }
            else
            {
                // Người dùng thường: chỉ trả chính họ
                users = _dbcontext.Users
                    .Where(u => u.Email == currentUserEmail)
                    .ToList();
            }

            var responses = users.Select(user =>
            {
                string? imageUrl = null;
                if (user.Avatar != null)
                {
                    string imageData = Convert.ToBase64String(user.Avatar);
                    imageUrl = $"data:image/jpeg;base64,{imageData}";
                }

                return new UserResponse
                {
                    Email = user.Email,
                    FullName = user.FullName,
                    Telephone = user.Telephone,
                    UserRole = user.UserRole,
                    Note = user.Note,
                    CreatedDate = user.CreatedDate,
                    UpdatedDate = user.UpdatedDate,
                    ImageUrl = imageUrl,
                    ServiceCode = user.ServiceCode
                };
            }).ToList();

            return responses;
        }







        public string UpdateUser(string email, string currentUserEmail, UserRequest request)
        {

            User user = _dbcontext.Users.Where(x => x.Email == email).FirstOrDefault();

            if (user == null)
            {
                return "User not found";
            }


            if (!string.Equals(email, currentUserEmail, StringComparison.OrdinalIgnoreCase))
            {
                return "You can only update your own information.";
            }

            try
            {
                if (request.FullName != null)
                {
                    user.FullName = request.FullName;
                }
                if (request.Password != null)
                {
                    user.Password = EncryptMd5.MD5Function(request.Password);
                }
                if (request.Telephone != null)
                {
                    user.Telephone = request.Telephone;
                }
                if (request.UserRole != null)
                {
                    user.UserRole = request.UserRole;
                }
                if (request.Note != null)
                {
                    user.Note = request.Note;
                }
                if (request.ServiceCode != null)
                {
                    user.ServiceCode = request.ServiceCode;
                }
                if (request.Avatar != null)
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        request.Avatar.CopyTo(stream);
                        user.Avatar = stream.ToArray();
                    }
                }
                //user.IsDeleted = request.IsDeleted;
                user.UpdatedDate = request.UpdatedDate;
                user.UpdatedUser = currentUserEmail;

                _dbcontext.SaveChanges();
                return "Updated";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
    }
}
