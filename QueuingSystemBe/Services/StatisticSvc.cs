using Npgsql;
using QueuingSystemBe.Models;
using QueuingSystemBe.ViewModels;

namespace QueuingSystemBe.Services
{
    public class StatisticSvc : IStatisticSvc
    {
        MyDbContext dbContext;
        private List<int> GetDaysInCurrentMonth(int month, int year)
        {
            int days = DateTime.DaysInMonth(year, month);
            return Enumerable.Range(1, days).ToList();
        }
        private (DateTime start, DateTime end) GetWeekRange(int week, int month, int year)
        {
            var firstDayOfMonth = new DateTime(year, month, 1);
            var daysInMonth = DateTime.DaysInMonth(year, month);

            int startDay = (week - 1) * 7 + 1;
            int endDay = Math.Min(week * 7, daysInMonth);

            DateTime start = new DateTime(year, month, startDay);
            DateTime end = new DateTime(year, month, endDay);

            return (start, end);
        }
        public StatisticSvc(MyDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public int GetAssignments()
        {
            int assignmentCount = dbContext.Assignments.Count();
            return assignmentCount;

        }

        public int GetAssignmentSkip()
        {
            var now = DateTimeOffset.UtcNow.Date;

            int skippedCount = dbContext.Assignments.Count(a => a.Status == 3 && a.ExpireDate < now);

            return skippedCount;
        }

        public int GetAssignmentUsed()
        {
            int UsedCount = dbContext.Assignments.Count(a => a.Status == 1);
            return UsedCount;
        }
        public int GetAssignmentWait()
        {
            var today = DateTimeOffset.UtcNow.Date;
            int WaitCount = dbContext.Assignments.Count(a => a.Status == 2 && a.AssignmentDate.Date == today);
            return WaitCount;
        }

        public DeviceStatistic GetDevice()
        {
            int deviceTotal = dbContext.Devices.Count();
            int active = dbContext.Devices.Count(d => d.OperationStatus == true);
            int inactive = deviceTotal - active;
            double activePercent = deviceTotal > 0 ? (active * 100) / deviceTotal : 0;
            return new DeviceStatistic
            {
                DeviceTotal = deviceTotal,
                Active = active,
                Inactive = inactive,
                ActivePercent = Math.Round(activePercent, 2)
            };
        }
        public ServiceStatistic GetService()
        {
            int serviceTotal = dbContext.Services.Count();
            int active = dbContext.Services.Count(s => s.IsInOperation == true);
            int inactive = serviceTotal - active;
            double activePercent = serviceTotal > 0 ? (active * 100) / serviceTotal : 0;
            return new ServiceStatistic
            {
                ServiceTotal = serviceTotal,
                Active = active,
                InActive = inactive,
                ActivePercent = Math.Round(activePercent, 2)
            };
        }

        public List<StatisticMonth> GetStatisticByMonth(int month)
        {
            var assignments = dbContext.Assignments.Where(x => x.AssignmentDate.Month == month && x.AssignmentDate.Year == DateTime.UtcNow.Year).ToList();
            List<StatisticMonth> myList = new List<StatisticMonth>();
            foreach (var item in GetDaysInCurrentMonth(month, DateTime.UtcNow.Year))
            {
                myList.Add(new StatisticMonth()
                {
                    Name = item.ToString(),
                    Value = assignments.Where(x => x.AssignmentDate.Day == item).Count()
                });
            }
            return myList;
        }
        public List<StatisticMonth> GetStatisticByWeek(int month)
        {
            int year = DateTime.UtcNow.Year;
            var assignments = dbContext.Assignments.Where(x => x.AssignmentDate.Month == month && x.AssignmentDate.Year == DateTime.UtcNow.Year).ToList();
            List<StatisticMonth> myList = new List<StatisticMonth>();
            foreach (var item in Enumerable.Range(1, 5))
            {
                var (start, end) = GetWeekRange(item, month, year);
                myList.Add(new StatisticMonth
                {
                    Name = item.ToString(),
                    Value = assignments.Count(x => x.AssignmentDate.Date >= start && x.AssignmentDate.Date <= end)
                });
            }
            return myList;
        }
        public List<StatisticMonth> GetStatisticByYear()
        {
            int year = DateTime.UtcNow.Year;
            var assignments = dbContext.Assignments.Where(x => x.AssignmentDate.Year == DateTime.UtcNow.Year).ToList();
            List<StatisticMonth> myList = new List<StatisticMonth>();
            foreach (var item in Enumerable.Range(1, 12))
            {
                myList.Add(new StatisticMonth
                {
                    Name = item.ToString(),
                    Value = assignments.Count(x => x.AssignmentDate.Month == item)
                });
            }
            return myList;
        }
    }
}
