using Microsoft.EntityFrameworkCore;
using QueuingSystemBe.Models;
using QueuingSystemBe.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace QueuingSystemBe.Services
{
    public class AssignmentSvc : IAssignmentSvc
    {
        private readonly MyDbContext _context;

        public AssignmentSvc(MyDbContext context)
        {
            _context = context;
        }

        public PagedResult<Assignment> GetAssignmentsByRole(string email, int page = 1, int pageSize = 10)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
                return new PagedResult<Assignment>();

            var role = user.UserRole ?? "";

            IQueryable<Assignment> query = _context.Assignments;

            if (role.Equals("Doctor", StringComparison.OrdinalIgnoreCase))
            {
                var nowUtc = DateTimeOffset.UtcNow;
                var startOfDay = new DateTimeOffset(nowUtc.DateTime.Date, TimeSpan.Zero);
                var endOfDay = startOfDay.AddDays(1);

                query = query.Where(a => a.AssignmentDate >= startOfDay && a.AssignmentDate < endOfDay && a.Status == 1);
            }

            var totalItems = query.Count();

            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            var items = query
                .OrderBy(a => a.AssignmentDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<Assignment>
            {
                Items = items,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize
            };
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
        public bool UpdateStatusToNext(string code, string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null || !string.Equals(user.UserRole, "Doctor"))
                return false;

            var assignment = _context.Assignments
                .FirstOrDefault(a => a.Code == code && (a.Status == 1 || a.Status == 2));

            if (assignment == null)
                return false;

            assignment.Status = 3;
            assignment.UpdatedDate = DateTimeOffset.UtcNow;

            _context.Assignments.Update(assignment);
            _context.SaveChanges();

            return true;
        }
        public bool UpdateStatusSequence(string code, string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null || !string.Equals(user.UserRole, "Doctor", StringComparison.OrdinalIgnoreCase))
                return false;

            var assignment = _context.Assignments
                .FirstOrDefault(a => a.Code == code && (a.Status == 1 || a.Status == 2));

            if (assignment == null)
                return false;

            if (assignment.Status == 1)
                assignment.Status = 2;
            else if (assignment.Status == 2)
                assignment.Status = 3;
            else
                return false;

            assignment.UpdatedDate = DateTimeOffset.UtcNow;
            _context.Assignments.Update(assignment);
            _context.SaveChanges();

            return true;
        }
        public PagedResult<Assignment> FilterAssignmentsForAdmin(AssignmentFilterRequest request)
        {
            var nowUtc = DateTimeOffset.UtcNow;
            var startOfDay = new DateTimeOffset(nowUtc.DateTime.Date, TimeSpan.Zero);
            var endOfDay = startOfDay.AddDays(1);

            var query = _context.Assignments
                    .Where(a => a.AssignmentDate >= startOfDay && a.AssignmentDate < endOfDay);

            if (!string.IsNullOrEmpty(request.ServiceCode))
                query = query.Where(a => a.ServiceCode.Contains(request.ServiceCode));

            if (request.Status.HasValue)
                query = query.Where(a => a.Status == request.Status.Value);

            if (!string.IsNullOrEmpty(request.Source))
                query = query.Where(a => a.DeviceCode == request.Source);

            if (!string.IsNullOrEmpty(request.Keyword))
                query = query.Where(a => a.CustomerName.Contains(request.Keyword));

            if (request.StartDate.HasValue)
                query = query.Where(a => a.AssignmentDate >= request.StartDate.Value);

            if (request.EndDate.HasValue)
                query = query.Where(a => a.AssignmentDate <= request.EndDate.Value);

            var totalItems = query.Count();

            var items = query
                .OrderByDescending(a => a.AssignmentDate)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return new PagedResult<Assignment>
            {
                TotalItems = totalItems,
                Page = request.Page,
                PageSize = request.PageSize,
                Items = items
            };
        }

    }
}
