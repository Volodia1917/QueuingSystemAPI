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

        public List<Assignment> GetAssignmentsByRole(string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
                return new List<Assignment>();

            var role = user.UserRole ?? "";
            var nowUtc = DateTimeOffset.UtcNow;
            var startOfDay = new DateTimeOffset(nowUtc.DateTime.Date, TimeSpan.Zero);
            var endOfDay = startOfDay.AddDays(1);
            if (role.Equals("Doctor"))
            {
                return _context.Assignments
                    .Where(a => a.Status == 1)
                    .Where(a => a.AssignmentDate >= startOfDay && a.AssignmentDate < endOfDay)
                    .OrderBy(a => a.AssignmentDate)
                    .ToList();
            }
            else
            {
                return _context.Assignments
                    .Where(a => a.AssignmentDate >= startOfDay && a.AssignmentDate < endOfDay)
                    .OrderBy(a => a.AssignmentDate)
                    .ToList();
            }
        }

        public Assignment GenerateNewAssignment(AssignmentCreateRequest request)
        {
            var now = DateTimeOffset.UtcNow;
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
                Status = 1,
                ServiceCode = request.ServiceCode,
                DeviceCode = request.DeviceCode,
                CreatedDate = now
            };

            _context.Assignments.Add(assignment);
            _context.SaveChanges();

            return assignment;
        }
        public bool UpdateStatusToProcessing(string code, string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null || !string.Equals(user.UserRole, "Doctor"))
                return false;

            var assignment = _context.Assignments.FirstOrDefault(a => a.Code == code && a.Status == 1);
            if (assignment == null)
                return false;

            assignment.Status = 2;
            assignment.UpdatedDate = DateTimeOffset.UtcNow;

            _context.Assignments.Update(assignment);
            _context.SaveChanges();

            return true;
        }

    }
}
