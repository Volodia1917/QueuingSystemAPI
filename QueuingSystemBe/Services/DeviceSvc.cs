using QueuingSystemBe.Models;
using QueuingSystemBe.ViewModels;

namespace QueuingSystemBe.Services
{
    public class DeviceSvc : IDeviceSvc
    {
        private readonly MyDbContext _context;

        public DeviceSvc(MyDbContext context)
        {
            _context = context;
        }

        public List<DeviceResponse> GetDevices(DeviceFilterRequest filter, out int total)
        {
            var query = _context.Devices.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                query = query.Where(d => d.DeviceCode.Contains(filter.Search) || d.DeviceName.Contains(filter.Search));
            }

            if (filter.Connected.HasValue)
            {
                query = query.Where(d => d.Connected == filter.Connected);
            }

            if (filter.OperationStatus.HasValue)
            {
                query = query.Where(d => d.OperationStatus == filter.OperationStatus);
            }

            total = query.Count();

            var result = query
                .Skip((filter.Page - 1) * filter.Limit)
                .Take(filter.Limit)
                .Select(d => new DeviceResponse
                {
                    DeviceCode = d.DeviceCode,
                    DeviceName = d.DeviceName,
                    IpAddress = d.IpAddress,
                    Connected = d.Connected,
                    OperationStatus = d.OperationStatus
                })
                .ToList();

            return result;
        }

        public bool AddDevice(DeviceRequest request)
        {
            if (_context.Devices.Any(d => d.DeviceCode == request.DeviceCode)) return false;

            var device = new Device
            {
                DeviceCode = request.DeviceCode,
                DeviceName = request.DeviceName,
                IpAddress = request.IpAddress,
                Connected = request.Connected,
                OperationStatus = request.OperationStatus,
                CreatedUser = request.CreatedUser,
                CreatedDate = request.CreatedDate
            };

            _context.Devices.Add(device);
            _context.SaveChanges();
            return true;
        }

        public bool UpdateDevice(string deviceCode, DeviceRequest request)
        {
            var device = _context.Devices.FirstOrDefault(d => d.DeviceCode == deviceCode);
            if (device == null) return false;

            device.DeviceName = request.DeviceName;
            device.IpAddress = request.IpAddress;
            device.Connected = request.Connected;
            device.OperationStatus = request.OperationStatus;
            device.UpdatedUser = request.UpdatedUser;
            device.UpdatedDate = request.UpdatedDate;

            _context.SaveChanges();
            return true;
        }

        public bool DeleteDevice(string deviceCode, string deletedBy)
        {
            var device = _context.Devices.FirstOrDefault(d => d.DeviceCode == deviceCode);
            if (device == null) return false;

            _context.Devices.Remove(device);
            _context.SaveChanges();
            return true;
        }
    }
}
