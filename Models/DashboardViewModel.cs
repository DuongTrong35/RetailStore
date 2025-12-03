namespace RetailStore.Models
{
    public class DashboardViewModel
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalProducts { get; set; }
        public int TotalCustomers { get; set; }

        public List<ChartDataPoint> RevenueChart { get; set; } = new List<ChartDataPoint>();
        public List<ProductStatistic> TopProducts { get; set; } = new List<ProductStatistic>();
        public List<RecentOrderViewModel> RecentOrders { get; set; } = new List<RecentOrderViewModel>();

        public List<int> PercentStatusOrder = new List<int> { 0, 0, 0, 0, 0 };
    }

    public class ChartDataPoint
    {
        public string Label { get; set; } = string.Empty; // "01/12", "02/12"
        public decimal Value { get; set; }
    }

    public class ProductStatistic
    {
        public string ProductName { get; set; } = string.Empty;
        public int TotalSold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class RecentOrderViewModel
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
    }
}
