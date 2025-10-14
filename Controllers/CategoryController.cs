using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailStore.Models;
namespace RetailStore.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(ApplicationDbContext context, ILogger<CategoryController> logger)
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
                var categoriesQuery = from c in _context.Categories
                                      select c;

                // Nếu có chuỗi tìm kiếm, thêm điều kiện Where vào truy vấn
                if (!String.IsNullOrEmpty(searchString))
                {
                    categoriesQuery = categoriesQuery.Where(c => c.CategoryName.Contains(searchString));
                }

                // Thực thi truy vấn và trả về danh sách kết quả cho View
                var categories = await categoriesQuery.ToListAsync();
                // Thêm logic kiểm tra AJAX vào đây
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView(categories);
                }

                return View(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Không thể lấy danh sách categories.");
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
        public async Task<IActionResult> Create(Category category)
        {
            // Kiểm tra xem dữ liệu gửi lên có hợp lệ không (dựa vào Data Annotations trong Model)
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Categories.Add(category);      // 1. Thêm category mới vào DbContext
                    await _context.SaveChangesAsync();      // 2. Lưu thay đổi vào database
                    return RedirectToAction("Index");       // 3. Chuyển hướng về trang danh sách
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi tạo category mới.");
                    // Có thể thêm lỗi vào ModelState để hiển thị cho người dùng
                    ModelState.AddModelError("", "Đã xảy ra lỗi, không thể lưu danh mục.");
                }
            }
            // Nếu dữ liệu không hợp lệ, trả về lại View với dữ liệu đã nhập
            return View(category);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound(); // Lỗi nếu không có ID
            }

            // Tìm category trong database theo ID
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound(); // Lỗi nếu không tìm thấy
            }

            // 1. Kiểm tra xem đây có phải là một yêu cầu AJAX không
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                // 2. Nếu là AJAX, chỉ trả về nội dung của view (không kèm layout)
                return PartialView(category);
            }

            // 3. Nếu là request thông thường, trả về view đầy đủ với layout
            return View(category);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoryId,CategoryName")] Category category)
        {
            // Kiểm tra xem ID từ URL có khớp với ID trong form không
            if (id != category.CategoryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category); // Đánh dấu đối tượng là đã thay đổi
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
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSelected(List<int> selectedIds)
        {
            if (selectedIds == null || !selectedIds.Any())
            {
                return RedirectToAction(nameof(Index));
            }

            // --- BƯỚC 1: TÌM VÀ CẬP NHẬT CÁC SẢN PHẨM LIÊN QUAN ---
            // Tìm tất cả sản phẩm có CategoryId nằm trong danh sách sẽ bị xóa.
            var productsToUpdate = await _context.Products
                                                 .Where(p => p.CategoryId != null && selectedIds.Contains((int)p.CategoryId))
                                                 .ToListAsync();

            if (productsToUpdate.Any())
            {
                // Lặp qua từng sản phẩm và set CategoryId về null
                foreach (var product in productsToUpdate)
                {
                    product.CategoryId = null;
                }
            }

            // --- BƯỚC 2: TÌM VÀ XÓA CÁC DANH MỤC ĐÃ CHỌN ---
            // Tìm các danh mục cần xóa (giống như code cũ của bạn).
            var categoriesToDelete = await _context.Categories
                                                   .Where(c => selectedIds.Contains(c.CategoryId))
                                                   .ToListAsync();

            if (categoriesToDelete.Any())
            {
                _context.Categories.RemoveRange(categoriesToDelete);
            }

            // --- BƯỚC 3: LƯU TẤT CẢ THAY ĐỔI ---
            // Chỉ cần gọi SaveChangesAsync một lần duy nhất.
            // EF Core sẽ tự động lưu cả việc cập nhật sản phẩm và xóa danh mục trong cùng 1 giao dịch.
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
