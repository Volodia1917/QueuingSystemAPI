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
                Service service = new Service();
                service.ServiceName = serviceRequest.ServiceName;
                service.Description = serviceRequest.Description;
                service.IsInOperation = serviceRequest.IsInOperation;
                service.CreatedDate = DateTimeOffset.UtcNow;
                service.CreatedUser = "Admin";

                // Lấy các mã hiện tại (dạng "001", "002", ...) và xử lý bằng LINQ trong bộ nhớ
                var usedCodes = _context.Services
                    .AsEnumerable() 
                    .Select(s => s.ServiceCode)
                    .Where(code => code.Length == 3 && code.All(char.IsDigit))
                    .Select(code => int.Parse(code))
                    .OrderBy(num => num)
                    .ToList();


                // Tìm mã trống nhỏ nhất
                int newCodeNum = 1;
                foreach (var codeNum in usedCodes)
                {
                    if (codeNum == newCodeNum)
                        newCodeNum++;
                    else
                        break;
                }

                
                if (newCodeNum > 999)
                    return "Error: max Service Number < 999).";

                string newCode = $"{newCodeNum:D3}";
                service.ServiceCode = newCode;

                _context.Services.Add(service);
                _context.SaveChanges();

                return $"Added Service ";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }




        public void ArchiveServiceToFile(Service service)
        {
            // Tạo thư mục nếu chưa có
            string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DeletedArchives");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Tạo nội dung dữ liệu cần lưu
            var archiveData = new
            {
                
                service.ServiceCode,
                service.ServiceName,
                service.Description,
                service.IsInOperation,
                service.DeletedUser,
                service.DeletedDate 
            };

            // Chuyển thành JSON
            string json = JsonConvert.SerializeObject(archiveData, Formatting.Indented);

            // Đặt tên file theo ServiceCode và thời gian
            string fileName = $"{service.ServiceCode}_{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.json";
            string fullPath = Path.Combine(folderPath, fileName);

            // Ghi file
            File.WriteAllText(fullPath, json);
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




        public List<ServiceRespone> GetServices(ServiceFilterDto filter, out int totalRecords)
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
            var result = services.Select(service => new ServiceRespone
            {
                ServiceCode = service.ServiceCode,
                ServiceName = service.ServiceName,
                Description = service.Description,
                IsInOperation = service.IsInOperation
                // Bổ sung trường khác nếu cần
            }).ToList();

            return result;
        }


        public List<ServiceRespone> GetServices1()
        {
            List<ServiceRespone> mylist = new List<ServiceRespone>();

            var ServiceList = _context.Services
                              .Where(s => s.DeletedDate == null)
                              .ToList();


            foreach (var service in ServiceList)
            {
                mylist.Add(new ServiceRespone()
                {
                    ServiceCode = service.ServiceCode,
                    ServiceName = service.ServiceName,
                    Description = service.Description,
                    IsInOperation = service.IsInOperation,


                    // Nếu Service có thêm trường nào khác thì bổ sung ở đây
                });
            }

            return mylist;
        }









    }
}

