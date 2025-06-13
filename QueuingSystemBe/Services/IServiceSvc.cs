
using QueuingSystemBe.Dtos;
using QueuingSystemBe.ViewModels;

namespace QueuingSystemBe.Services
{
    public interface IServiceSvc
    {
        public string AddService(ServiceRequest serviceRequest);
        public string UpdateService(string ServiceCode, ServiceRequest serviceRequest);
        public string DeleteService(string ServiceCode);
        public List<ServiceResponse> GetSelectService(ServiceFilterDto filter, out int totalRecords);
        public List<ServiceResponse> GetAllServices();

    }
}
