using QueuingSystemBe.Models;
using QueuingSystemBe.ViewModels;

namespace QueuingSystemBe.Services
{
    public interface IStatisticSvc
    {
        public int GetAssignments();
        public int GetAssignmentUsed();
        public int GetAssignmentWait();
        public int GetAssignmentSkip();
        public DeviceStatistic GetDevice();
        public ServiceStatistic GetService();
        public List<StatisticMonth> GetStatisticByMonth(int month);
        public List<StatisticMonth> GetStatisticByWeek(int month);
        public List<StatisticMonth> GetStatisticByYear();

    }
}
