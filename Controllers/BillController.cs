using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailStore.Models;

namespace RetailStore.Controllers
{
    public class BillController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BillController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string searchString, DateTime? dateFrom, DateTime? dateTo)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["DateFrom"] = dateFrom?.ToString("yyyy-MM-dd");
            ViewData["DateTo"] = dateTo?.ToString("yyyy-MM-dd");

            var orders = _context.Orders.AsQueryable();

            // Lọc theo mã hóa đơn
            if (!string.IsNullOrEmpty(searchString))
            {
                orders = orders.Where(o => o.OrderId.ToString().Contains(searchString));
            }

            //  Lọc từ ngày
            if (dateFrom.HasValue)
            {
                orders = orders.Where(o => o.OrderDate >= dateFrom.Value);
            }

            //  Lọc đến ngày
            if (dateTo.HasValue)
            {
                var dt = dateTo.Value.Date.AddDays(1).AddSeconds(-1);
                orders = orders.Where(o => o.OrderDate <= dt);
            }

            return View(orders.OrderByDescending(o => o.OrderDate).ToList());
        }

        //public IActionResult Index(string searchString)
        //{
        //    var orders = _context.Orders.AsQueryable();

        //    if (!string.IsNullOrEmpty(searchString))
        //    {
        //        orders = orders.Where(o => o.OrderId.ToString().Contains(searchString));
        //    }

        //    return View(orders.OrderByDescending(o => o.OrderDate).ToList());
        //}

        public IActionResult Details(int id)
        {
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Payments)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null) return NotFound();

            return View(order);
        }

        [HttpPost]
        public JsonResult MarkPaid(int id)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return Json(new { success = false, message = "Không tìm thấy hóa đơn" });
            }

            order.Status = "paid";
            _context.SaveChanges();

            return Json(new { success = true });
        }

    }
}
