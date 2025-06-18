using QueuingSystemBe.Models;
using QueuingSystemBe.ViewModels;

namespace QueuingSystemBe.Services
{
    public interface IStatisticSvc
    {
        public DashboardStatistic GetDashboardOverview(string period = "day", int? month = null);
        public NumbersOverview GetNumbersOverview();
        public List<ChartData> GetChartData(string name = "day", int? month = null);
        public OverallSummary GetOverallSummary();

    }
}
