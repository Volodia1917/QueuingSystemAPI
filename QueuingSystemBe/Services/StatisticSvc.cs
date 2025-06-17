using Npgsql;
using QueuingSystemBe.Models;
using QueuingSystemBe.ViewModels;

namespace QueuingSystemBe.Services
{
    public class StatisticSvc : IStatisticSvc
    {
        private readonly MyDbContext dbContext;

        public StatisticSvc(MyDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        private DateTimeOffset Today => DateTimeOffset.UtcNow.Date;
        private DateTimeOffset Now => DateTimeOffset.UtcNow;

        private double CalculatePercentageChange(int current, int previous)
        {
            if (previous == 0) return current > 0 ? 100 : 0;
            return Math.Round(((double)(current - previous) / previous) * 100, 1);
        }

        private void AutoUpdateSkippedStatus()
        {
            var now = Now;
            var expiredWaitingAssignments = dbContext.Assignments
                .Where(a => a.Status == 2 && a.ExpireDate < now)
                .ToList();

            foreach (var assignment in expiredWaitingAssignments)
            {
                assignment.Status = 3;
            }

            if (expiredWaitingAssignments.Any())
            {
                dbContext.SaveChanges();
            }
        }

        public DashboardStatistic GetDashboardOverview(string period = "day", int? month = null)
        {
            AutoUpdateSkippedStatus();

            var chartData = GetChartData(period, month);

            return new DashboardStatistic
            {
                NumbersOverview = GetNumbersOverview(),
                ChartData = chartData,
                OverallSummary = GetOverallSummary()
            };
        }

        public NumbersOverview GetNumbersOverview()
        {
            var today = Today;
            var yesterday = today.AddDays(-1);

            return new NumbersOverview
            {
                Total = CreateStatisticCard("Số thứ tự đã cấp", today, yesterday, a => true, "ticket-issued", "#E1F0FF"),
                Used = CreateStatisticCard("Số thứ tự đã sử dụng", today, yesterday, a => a.Status == 1, "ticket-used", "#E1F7E8"),
                Waiting = CreateStatisticCard("Số thứ tự đang chờ", today, yesterday, a => a.Status == 2, "ticket-waiting", "#FFF3E9"),
                Skipped = CreateStatisticCard("Số thứ tự đã bỏ qua", today, yesterday, a => a.Status == 3, "ticket-skipped", "#FEE9E9", true)
            };
        }

        private StatisticCard CreateStatisticCard(
            string title,
            DateTimeOffset today,
            DateTimeOffset yesterday,
            Func<Assignment, bool> predicate,
            string icon,
            string color,
            bool useExpireDate = false)
        {
            int todayCount = dbContext.Assignments.Where(a => (useExpireDate
                 ? a.ExpireDate.Date == today.Date
                 : a.AssignmentDate.Date == today.Date)).Where(predicate).Count();

            int yesterdayCount = dbContext.Assignments.Where(a => (useExpireDate
            ? a.ExpireDate.Date == yesterday.Date
            : a.AssignmentDate.Date == yesterday.Date)).Where(predicate).Count();

            return new StatisticCard
            {
                Title = title,
                Value = todayCount,
                PercentageChange = CalculatePercentageChange(todayCount, yesterdayCount),
                IsIncrease = todayCount >= yesterdayCount,
                Icon = icon,
                Color = color
            };
        }

        public List<ChartData> GetChartData(string name = "day", int? month = null)
        {
            var chartData = new List<ChartData>();
            var now = Now;
            int currentYear = now.Year;
            int targetMonth = month ?? now.Month;

            if (name.ToLower() == "day")
            {
                int daysInMonth = DateTime.DaysInMonth(currentYear, targetMonth);
                for (int day = 1; day <= daysInMonth; day++)
                {
                    var dateStart = new DateTimeOffset(new DateTime(currentYear, targetMonth, day, 0, 0, 0, DateTimeKind.Utc));
                    var dateEnd = dateStart.AddDays(1);

                    var count = dbContext.Assignments.Count(a => a.AssignmentDate >= dateStart && a.AssignmentDate < dateEnd);
                    chartData.Add(new ChartData { Date = dateStart.ToString("dd/MM"), Value = count, Period = "day" });
                }
            }
            else if (name.ToLower() == "week")
            {
                var firstDay = new DateTimeOffset(new DateTime(currentYear, targetMonth, 1, 0, 0, 0, DateTimeKind.Utc));
                var lastDay = firstDay.AddMonths(1).AddDays(-1);

                var currentWeekStart = firstDay;
                int weekIndex = 1;

                while (currentWeekStart <= lastDay)
                {
                    var currentWeekEnd = currentWeekStart.AddDays(7);
                    if (currentWeekEnd > lastDay.AddDays(1)) currentWeekEnd = lastDay.AddDays(1);

                    var count = dbContext.Assignments.Count(a =>
                        a.AssignmentDate >= currentWeekStart && a.AssignmentDate < currentWeekEnd);

                    chartData.Add(new ChartData { Date = $"Tuần {weekIndex}", Value = count, Period = "week" });

                    currentWeekStart = currentWeekStart.AddDays(7);
                    weekIndex++;
                }
            }
            else if (name.ToLower() == "month")
            {
                for (int i = 1; i <= 12; i++)
                {
                    var monthStart = new DateTimeOffset(new DateTime(currentYear, i, 1, 0, 0, 0, DateTimeKind.Utc));
                    var monthEnd = monthStart.AddMonths(1);

                    var count = dbContext.Assignments.Count(a =>
                        a.AssignmentDate >= monthStart && a.AssignmentDate < monthEnd);

                    chartData.Add(new ChartData { Date = $"{i:00}/{currentYear}", Value = count, Period = "month" });
                }
            }
            else if (name.ToLower() == "year")
            {
                for (int i = 4; i >= 0; i--)
                {
                    var year = currentYear - i;
                    var yearStart = new DateTimeOffset(new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc));
                    var yearEnd = yearStart.AddYears(1);

                    var count = dbContext.Assignments.Count(a =>
                        a.AssignmentDate >= yearStart && a.AssignmentDate < yearEnd);

                    chartData.Add(new ChartData { Date = year.ToString(), Value = count, Period = "year" });
                }
            }

            return chartData;
        }

        public OverallSummary GetOverallSummary()
        {
            var deviceTotal = dbContext.Devices.Count();
            var deviceActive = dbContext.Devices.Count(d => d.OperationStatus == true);
            var deviceInactive = deviceTotal - deviceActive;

            var serviceTotal = dbContext.Services.Count();
            var serviceActive = dbContext.Services.Count(s => s.IsInOperation == true);
            var serviceInactive = serviceTotal - serviceActive;

            var numbersTotal = dbContext.Assignments.Count();
            var numbersUsed = dbContext.Assignments.Count(a => a.Status == 1);
            var numbersWaiting = dbContext.Assignments.Count(a => a.Status == 2);
            var numbersSkipped = dbContext.Assignments.Count(a => a.Status == 3);

            return new OverallSummary
            {
                Devices = new SummaryItem
                {
                    Label = "Thiết bị",
                    Color = "#ff8c00",
                    Details =
                    [
                        new DetailItem { Name = "Đang hoạt động", Value = deviceActive.ToString("#,##0"), Color = "#ff8c00" },
                        new DetailItem { Name = "Ngừng hoạt động", Value = deviceInactive.ToString("#,##0"), Color = "#505050" }
                    ],
                    Total = deviceTotal,
                    ActivePercentage = deviceTotal > 0 ? Math.Round((double)deviceActive / deviceTotal * 100, 1) : 0
                },
                Services = new SummaryItem
                {
                    Label = "Dịch vụ",
                    Color = "#007bff",
                    Details =
                    [
                        new DetailItem { Name = "Đang hoạt động", Value = serviceActive.ToString("#,##0"), Color = "#007bff" },
                        new DetailItem { Name = "Ngừng hoạt động", Value = serviceInactive.ToString("#,##0"), Color = "#505050" }
                    ],
                    Total = serviceTotal,
                    ActivePercentage = serviceTotal > 0 ? Math.Round((double)serviceActive / serviceTotal * 100, 1) : 0
                },
                NumbersGiven = new SummaryItem
                {
                    Label = "Cấp số",
                    Color = "#28a745",
                    Details =
                    [
                        new DetailItem { Name = "Đã sử dụng", Value = numbersUsed.ToString("#,##0"), Color = "#28a745" },
                        new DetailItem { Name = "Đang chờ", Value = numbersWaiting.ToString("#,##0"), Color = "#ffc107" },
                        new DetailItem { Name = "Bỏ qua", Value = numbersSkipped.ToString("#,##0"), Color = "#505050" }
                    ],
                    Total = numbersTotal,
                    ActivePercentage = numbersTotal > 0 ? Math.Round((double)numbersUsed / numbersTotal * 100, 1) : 0
                }
            };
        }
    }
}
