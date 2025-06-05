using Npgsql;
using QueuingSystemBe.Models;
using QueuingSystemBe.ViewModels;

namespace QueuingSystemBe.Services
{
    public class StatisticSvc:IStatisticSvc
    {
        MyDbContext dbContext;
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

            int skippedCount = dbContext.Assignments.Count(a => a.Status == 3  && a.ExpireDate < now);

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
            int active = dbContext.Devices.Count(d=>d.OperationStatus == true);
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
    }
}
