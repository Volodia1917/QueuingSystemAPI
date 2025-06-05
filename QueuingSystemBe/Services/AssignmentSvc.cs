using Microsoft.EntityFrameworkCore;
using QueuingSystemBe.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QueuingSystemBe.Services
{
    public class AssignmentSvc : IAssignmentSvc
    {
        private readonly MyDbContext _context;

        public AssignmentSvc(MyDbContext context)
        {
            _context = context;
        }

        public List<Assignment> GetAll()
        {
            return _context.Assignments
                .Include(a => a.Service)
                .Include(a => a.Device)
                .ToList();
        }

        public List<Assignment> GetAssignmentsByRole(string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
                return new List<Assignment>();

            var role = user.UserRole ?? "";

            if (role.Equals("Doctor", StringComparison.OrdinalIgnoreCase))
            {
                return _context.Assignments
                    .Where(a => a.Status == 0)
                    .OrderBy(a => a.AssignmentDate)
                    .Include(a => a.Service)
                    .Include(a => a.Device)
                    .ToList();
            }
            else if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                return _context.Assignments
                    .OrderBy(a => a.AssignmentDate)
                    .Include(a => a.Service)
                    .Include(a => a.Device)
                    .ToList();
            }
            else
            {
                return new List<Assignment>();
            }
        }

        public Assignment GenerateNewAssignment(AssignmentCreateRequest request)
        {
            var now = DateTime.UtcNow;
            var datePrefix = now.ToString("yyMMdd");

            var todayCodes = _context.Assignments
                .Where(a => a.Code.StartsWith(datePrefix))
                .OrderByDescending(a => a.Code)
                .Select(a => a.Code)
                .ToList();

            int nextNumber = 1;
            if (todayCodes.Any())
            {
                var lastCode = todayCodes.First();
                var lastNumber = int.Parse(lastCode.Substring(6));
                nextNumber = lastNumber + 1;
            }

            if (nextNumber > 9999)
                throw new InvalidOperationException("Đã đạt giới hạn số cấp trong ngày.");

            var serial = nextNumber.ToString("D4");
            var fullCode = $"{datePrefix}{serial}";

            var assignment = new Assignment
            {
                Code = fullCode,
                CustomerName = request.CustomerName,
                CustomerEmail = request.CustomerEmail,
                Telephone = request.Telephone,
                AssignmentDate = now,
                ExpireDate = now.AddHours(4),
                Status = 0,
                ServiceCode = request.ServiceCode,
                DeviceCode = request.DeviceCode,
                CreatedDate = now
            };

            _context.Assignments.Add(assignment);
            _context.SaveChanges();

            return assignment;
        }

    }
}
