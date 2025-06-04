using QueuingSystemBe.Models;
using QueuingSystemBe.ViewModels;
using QueuingSystemBe.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;

namespace QueuingSystemBe.Services
{
    public class UserSvc : IUserSvc
    {
        private MyDbContext _dbcontext;
        public UserSvc(MyDbContext dbcontext)
        {
            _dbcontext = dbcontext;
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
                user.IsDeleted = request.IsDeleted;
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

        public string DeleteUser(string email, string currentUserEmail, UserRequest request)
        {

            User user = _dbcontext.Users.Where(x => x.Email == email && x.IsDeleted == false).FirstOrDefault();
            if (user == null) return "Not Found";

            try
            {
                user.IsDeleted = true;
                user.DeletedUser = currentUserEmail;
                user.DeletedDate =request.DeletedDate;

                _dbcontext.Users.Update(user);
                _dbcontext.SaveChanges();
                return "Deleted";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }



        public List<UserResponse> GetUser(string? email, string currentUserEmail)
        {
            // Lấy user hiện tại theo currentUserEmail để check role
            User user = _dbcontext.Users.Where(u => u.Email == currentUserEmail && u.IsDeleted != true).FirstOrDefault();

            if (user == null)
                return new List<UserResponse>();

            List<User> users;

            if (string.Equals(user.UserRole, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                // Admin: trả về tất cả user chưa bị xoá
                users = _dbcontext.Users.Where(u => u.IsDeleted != true).ToList();
            }
            else
            {
                // Người dùng thường: chỉ trả chính họ
                users = _dbcontext.Users.Where(u => u.Email == currentUserEmail && u.IsDeleted != true).ToList();
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
                    ImageUrl = imageUrl
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
                if (request.Avatar != null)
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        request.Avatar.CopyTo(stream);
                        user.Avatar = stream.ToArray();
                    }
                }
                user.IsDeleted = request.IsDeleted;
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
