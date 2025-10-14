using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailStore.Models;

namespace RetailStore.Controllers
{
    public class StaffController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StaffController> _logger;
        public StaffController(ApplicationDbContext context, ILogger<StaffController> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<IActionResult> Index(string searchString)
        {
            try
            {
                ViewData["CurrentFilter"] = searchString;

                var staffsQuery = _context.Users.Where(u => u.Role == "staff");

                if (!String.IsNullOrEmpty(searchString))
                {
                    staffsQuery = staffsQuery.Where(u => u.FullName.Contains(searchString));
                }

                var staffs = await staffsQuery.ToListAsync();

                // 1. Kiểm tra xem đây có phải là một yêu cầu AJAX không
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    // 2. Nếu là AJAX, chỉ trả về nội dung của view (không kèm layout)
                    return PartialView(staffs);
                }

                // 3. Nếu là request thông thường, trả về view đầy đủ với layout
                return View(staffs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Không thể lấy danh sách nhân viên.");
                return View("Error");
            }
        }
    }
}
