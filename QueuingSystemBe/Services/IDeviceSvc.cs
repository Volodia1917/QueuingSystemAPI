using QueuingSystemBe.ViewModels;

namespace QueuingSystemBe.Services
{
    public interface IDeviceSvc
    {
        List<DeviceResponse> GetDevices(DeviceFilterRequest filter, out int total);
        bool AddDevice(DeviceRequest request);
        bool UpdateDevice(string deviceCode, DeviceRequest request);
        bool DeleteDevice(string deviceCode, string deletedBy);
    }
}
