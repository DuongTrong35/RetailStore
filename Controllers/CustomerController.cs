using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailStore.Models;

namespace RetailStore.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(ApplicationDbContext context, ILogger<CustomerController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            try
            {
                // Lưu lại chuỗi tìm kiếm để hiển thị lại trên View
                ViewData["CurrentFilter"] = searchString;

                // Bắt đầu với một truy vấn cơ bản, chưa thực thi
                var customersQuery = from c in _context.Customers
                                     select c;

                // Nếu có chuỗi tìm kiếm, thêm điều kiện Where vào truy vấn
                if (!String.IsNullOrEmpty(searchString))
                {
                    customersQuery = customersQuery.Where(c => c.Name.Contains(searchString) || 
                                                               c.Phone.Contains(searchString) ||
                                                               c.Email.Contains(searchString));
                }

                // Thực thi truy vấn và trả về danh sách kết quả cho View
                var customers = await customersQuery.ToListAsync();

                // Thêm logic kiểm tra AJAX vào đây
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView(customers);
                }

                return View(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Không thể lấy danh sách khách hàng.");
                return View("Error");
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView(); // Trả về form tạo mới, không có layout
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            // Kiểm tra xem dữ liệu gửi lên có hợp lệ không (dựa vào Data Annotations trong Model)
            if (ModelState.IsValid)
            {
                try
                {
                    // Set audit fields
                    customer.CreatedAt = DateTime.Now;
                    customer.CreatedBy = HttpContext.Session.GetString("UserName") ?? "System";
                    customer.UpdatedAt = DateTime.Now;
                    customer.UpdatedBy = HttpContext.Session.GetString("UserName") ?? "System";

                    _context.Customers.Add(customer);      // 1. Thêm customer mới vào DbContext
                    await _context.SaveChangesAsync();      // 2. Lưu thay đổi vào database
                    return RedirectToAction("Index");       // 3. Chuyển hướng về trang danh sách
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi tạo khách hàng mới.");
                    // Có thể thêm lỗi vào ModelState để hiển thị cho người dùng
                    ModelState.AddModelError("", "Đã xảy ra lỗi, không thể lưu khách hàng.");
                }
            }
            // Nếu dữ liệu không hợp lệ, trả về lại View với dữ liệu đã nhập
            return View(customer);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound(); // Lỗi nếu không có ID
            }

            // Tìm customer trong database theo ID
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound(); // Lỗi nếu không tìm thấy
            }

            // 1. Kiểm tra xem đây có phải là một yêu cầu AJAX không
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                // 2. Nếu là AJAX, chỉ trả về nội dung của view (không kèm layout)
                return PartialView(customer);
            }

            // 3. Nếu là request thông thường, trả về view đầy đủ với layout
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CustomerId,Name,Phone,Email,Address,LoyaltyPoints,CustomerTier")] Customer customer)
        {
            // Kiểm tra xem ID từ URL có khớp với ID trong form không
            if (id != customer.CustomerId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Set audit fields
                    customer.UpdatedAt = DateTime.Now;
                    customer.UpdatedBy = HttpContext.Session.GetString("UserName") ?? "System";

                    _context.Update(customer); // Đánh dấu đối tượng là đã thay đổi
                    await _context.SaveChangesAsync(); // Lưu thay đổi vào database
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Xử lý lỗi nếu có ai đó đã sửa record này cùng lúc
                    // (Phần nâng cao, có thể bỏ qua lúc đầu)
                    throw;
                }
                return RedirectToAction(nameof(Index)); // Chuyển về trang danh sách
            }

            // Nếu dữ liệu không hợp lệ, hiển thị lại form với lỗi
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSelected(List<int> selectedIds)
        {
            if (selectedIds == null || !selectedIds.Any())
            {
                return RedirectToAction(nameof(Index));
            }

            // Tìm các khách hàng cần xóa
            var customersToDelete = await _context.Customers
                                                   .Where(c => selectedIds.Contains(c.CustomerId))
                                                   .ToListAsync();

            if (customersToDelete.Any())
            {
                _context.Customers.RemoveRange(customersToDelete);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView(customer);
            }

            return View(customer);
        }
    }
}
