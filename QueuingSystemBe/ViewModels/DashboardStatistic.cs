namespace QueuingSystemBe.ViewModels
{
    public class DashboardStatistic
    {
        public required NumbersOverview NumbersOverview { get; set; }
        public required List<ChartData> ChartData { get; set; }
        public required OverallSummary OverallSummary { get; set; }
    }

    public class NumbersOverview
    {
        public required StatisticCard Total { get; set; }
        public required StatisticCard Used { get; set; }
        public required StatisticCard Waiting { get; set; }
        public required StatisticCard Skipped { get; set; }
    }

    public class StatisticCard
    {
        public required string Title { get; set; }
        public int Value { get; set; }
        public double PercentageChange { get; set; }
        public bool IsIncrease { get; set; }
        public required string Icon { get; set; }
        public required string Color { get; set; }
    }

    public class ChartData
    {
        public required string Date { get; set; }
        public int Value { get; set; }
        public required string Period { get; set; } // day, week, month, year
    }
    public class OverallSummary
    {
        public required SummaryItem Devices { get; set; }
        public required SummaryItem Services { get; set; }
        public required SummaryItem NumbersGiven { get; set; }
    }
    public class SummaryItem
    {
        public required string Label { get; set; }
        public required string Color { get; set; }
        public List<DetailItem> Details { get; set; } = new List<DetailItem>();
        public int Total { get; set; }
        public double ActivePercentage { get; set; }
    }

    public class DetailItem
    {
        public required string Name { get; set; }
        public string Value { get; set; } = "0";
        public required string Color { get; set; }
    }
}
