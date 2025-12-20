namespace Myapp.DashboardStats
{
    public class UserStatsDto
    {
        public int TotalClients { get; set; }
        public int TotalProducts { get; set; }
        public int ActiveClients { get; set; }
        public int TotalTransactions { get; set; }
        public double TotalRevenue { get; set; }
    }

    public class DashboardStatsDto : UserStatsDto
    {
        public double AverageTransactionValue { get; set; }
        public double ClientConversionRate { get; set; }
        public List<MonthlyRevenueDto>? MonthlyRevenue { get; set; }
    }

    public class MonthlyRevenueDto
    {
        public string? Month { get; set; }
        public double Revenue { get; set; }
    }
}
