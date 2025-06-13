using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QueuingSystemBe.Models;
using QueuingSystemBe.ViewModels;
using Newtonsoft.Json;
using System.IO;
using System.Text.Json;
using QueuingSystemBe.Dtos;

namespace QueuingSystemBe.Services
{
    public class ServiceSvc : IServiceSvc
    {
        private MyDbContext _context;
        public ServiceSvc(MyDbContext context)
        {
            _context = context;
        }
        public string AddService(ServiceRequest serviceRequest)
        {
            try
            {
                var existingService = _context.Services
                    .FirstOrDefault(s => s.ServiceCode == serviceRequest.ServiceCode);

                if (existingService != null)
                {
                    return $"Error: ServiceCode '{serviceRequest.ServiceCode}' already exists.";
                }
                Service service = new Service();
                service.ServiceCode = serviceRequest.ServiceCode;
                service.ServiceName = serviceRequest.ServiceName;
                service.Description = serviceRequest.Description;
                service.IsInOperation = serviceRequest.IsInOperation;
                service.CreatedDate = DateTimeOffset.UtcNow;
                service.CreatedUser = "Admin";
                _context.Services.Add(service);
                _context.SaveChanges();

                return $"Added Service ";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }




        public string DeleteService(string ServiceCode)
        {
            try
            {
                Service service = _context.Services.Where(x => x.ServiceCode == ServiceCode).FirstOrDefault();
                if (service != null)
                {
                    service.DeletedDate = DateTime.UtcNow;
                    service.DeletedUser = "Admin";

                    _context.Services.Remove(service);
                    _context.SaveChanges();
                    return "Deleted";
                }

                else
                {
                    return "Service not found";
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }




        public string UpdateService(string ServiceCode, ServiceRequest serviceRequest)
        {
            try
            {
                Service service = _context.Services.Where(x => x.ServiceCode == ServiceCode).FirstOrDefault();
                if (service != null)
                {
                    if (serviceRequest.ServiceName != null)
                        service.ServiceName = serviceRequest.ServiceName;
                    if (serviceRequest.Description != null)
                        service.Description = (string)serviceRequest.Description;
                    if (serviceRequest.IsInOperation != null)
                        service.IsInOperation = (bool)serviceRequest.IsInOperation;
                    service.UpdatedDate = DateTimeOffset.UtcNow;
                    service.UpdatedUser = "Admin";
                    _context.SaveChanges();
                    return "Updated";
                }
                else
                {
                    return "Service not found ";
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }




        public List<ServiceResponse> GetSelectService(ServiceFilterDto filter, out int totalRecords)
        {
            var query = _context.Services
                .Where(s => s.DeletedDate == null);

            // Lọc trạng thái hoạt động
            if (filter.IsInOperation.HasValue)
            {
                query = query.Where(s => s.IsInOperation == filter.IsInOperation.Value);
            }

            // Lọc theo ngày (giả sử dùng CreatedDate, bạn có thể đổi)
            if (filter.StartDate.HasValue)
            {
                query = query.Where(s => s.CreatedDate >= filter.StartDate.Value);
            }
            if (filter.EndDate.HasValue)
            {
                query = query.Where(s => s.CreatedDate <= filter.EndDate.Value);
            }

            // Lọc từ khóa
            if (!string.IsNullOrEmpty(filter.Keyword))
            {
                var keyword = filter.Keyword.ToLower();
                query = query.Where(s =>
                    s.ServiceCode.ToLower().Contains(keyword) ||
                    s.ServiceName.ToLower().Contains(keyword) ||
                    (s.Description != null && s.Description.ToLower().Contains(keyword))
                );
            }

            // Tổng số bản ghi trước khi phân trang
            totalRecords = query.Count();

            // Phân trang
            var services = query
                .OrderByDescending(s => s.CreatedDate)
                .Skip((filter.Page - 1) * filter.Limit)
                .Take(filter.Limit)
                .ToList();

            // Mapping
            var result = services.Select(service => new ServiceResponse
            {
                ServiceCode = service.ServiceCode,
                ServiceName = service.ServiceName,
                Description = service.Description,
                IsInOperation = service.IsInOperation
                // Bổ sung trường khác nếu cần
            }).ToList();

            return result;
        }


        public List<ServiceResponse> GetAllServices()
        {
            List<ServiceResponse> mylist = new List<ServiceResponse>();

            var ServiceList = _context.Services
                              .Where(s => s.DeletedDate == null)
                              .ToList();


            foreach (var service in ServiceList)
            {
                mylist.Add(new ServiceResponse()
                {
                    ServiceCode = service.ServiceCode,
                    ServiceName = service.ServiceName,
                    Description = service.Description,
                    IsInOperation = service.IsInOperation,
                    CreatedDate = service.CreatedDate,


                    // Nếu Service có thêm trường nào khác thì bổ sung ở đây
                });
            }

            return mylist;
        }









    }
}

