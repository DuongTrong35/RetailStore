using Microsoft.AspNetCore.Mvc;
using RetailStore.Models;
using Microsoft.EntityFrameworkCore;
namespace RetailStore.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(ApplicationDbContext context, ILogger<DashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int days = 30)
        {
            try
            {
                ViewBag.SelectedDays = days;
                var viewModel = new DashboardViewModel
                {
                    TotalRevenue = await GetTotalRevenue(),
                    TotalOrders = await GetTotalOrders(),
                    TotalProducts = await GetTotalProducts(),
                    TotalCustomers = await GetTotalCustomers(),
                    RevenueChart = await GetRevenueChartData(days),
                    TopProducts = await GetTopSellingProducts(5),
                    RecentOrders = await GetRecentOrders(10),
                    PercentStatusOrder = await getPercentStatusOrder(days)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải dashboard");
                return View("Error");
            }
        }

        private async Task<decimal> GetTotalRevenue()
        {
            //var startDate = DateTime.Now.AddDays(-days);

            return await _context.Orders
                //.Where(o => o.OrderDate >= startDate)
                .SumAsync(o => o.TotalAmount) ?? 0;
        }

        private async Task<int> GetTotalOrders()
        {
            //var startDate = DateTime.Now.AddDays(-days);

            return await _context.Orders
                //.Where(o => o.OrderDate >= startDate)
                .CountAsync();
        }

        private async Task<int> GetTotalProducts()
        {
             return await _context.Products.CountAsync();
        }

        private async Task<int> GetTotalCustomers()
        {
             return await _context.Customers.CountAsync();
        }

        private async Task<List<ChartDataPoint>> GetRevenueChartData(int days)
        {
            var startDate = DateTime.Now.AddDays(-days);

            var data = await _context.Orders
                .Where(o => o.OrderDate >= startDate)
                .ToListAsync(); // lấy dữ liệu về trước

            return data
                .GroupBy(o => o.OrderDate?.Date ?? DateTime.MinValue)
                .Select(g => new ChartDataPoint
                {
                    Label = g.Key.ToString("dd/MM"), // xử lý format ở client
                    Value = g.Sum(o => o.TotalAmount ?? 0)
                })
                .OrderBy(x => x.Label)
                .ToList();
        }

        private async Task<List<int>> getPercentStatusOrder(int days)
        {
            var startDate = DateTime.Now.AddDays(-days);

            // Lấy danh sách đơn hàng trong khoảng thời gian
            var orders = await _context.Orders
                .Where(o => o.OrderDate >= startDate)
                .ToListAsync();

            int total = orders.Count;
            if (total == 0) return new List<int> { 0, 0, 0 };

            int pendingCount = orders.Count(o => o.Status == "pending");
            int paidCount = orders.Count(o => o.Status == "paid");
            int canceledCount = orders.Count(o => o.Status == "canceled");

            // Tính phần trăm (làm tròn xuống)
            int pendingPercent = (int)Math.Round((double)pendingCount * 100 / total);
            int paidPercent = (int)Math.Round((double)paidCount * 100 / total);
            int canceledPercent = (int)Math.Round((double)canceledCount * 100 / total);

            return new List<int> { pendingPercent, paidPercent, canceledPercent };
        }


        private async Task<List<ProductStatistic>> GetTopSellingProducts(int top)
        {
            return await (
                from oi in _context.OrderItems
                join o in _context.Orders on oi.OrderId equals o.OrderId
                join p in _context.Products on oi.ProductId equals p.ProductId
                where o.Status == "paid"   // chỉ lấy đơn hàng đã thanh toán
                group oi by new { oi.ProductId, p.ProductName } into g
                select new ProductStatistic
                {
                    ProductName = g.Key.ProductName,
                    TotalSold = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.Quantity * x.Price)
                }
            )
            .OrderByDescending(p => p.TotalSold)
            .Take(top)
            .ToListAsync();
        }

        private async Task<List<RecentOrderViewModel>> GetRecentOrders(int count)
        {
            return await (from o in _context.Orders
                          join c in _context.Customers on o.CustomerId equals c.CustomerId
                          orderby o.OrderDate descending
                          select new RecentOrderViewModel
                          {
                              OrderId = o.OrderId,
                              CustomerName = c.Name,
                              Status = o.Status,
                              TotalAmount = o.TotalAmount ??0,
                          })
              .Take(count)
              .ToListAsync();

        }
    }
}
