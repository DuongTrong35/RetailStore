using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

                var staffsQuery = _context.Users.Where(u => u.Role != "admin");

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

        public IActionResult Create()
        {
            var model = new User();

            ViewBag.Roles = new SelectList(new[] { "staff", "sales", "importer"});

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView(model);
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Username,Email,Password,FullName,Role")] User user)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    user.CreatedAt = DateTime.Now;


                    _context.Add(user);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo tài khoản mới.");
                ModelState.AddModelError("", "Không thể lưu tài khoản, vui lòng thử lại.");
            }

            ViewBag.Roles = new SelectList(new[] { "staff", "sales", "importer" });

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView(user);
            }
            return View(user);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            ViewBag.Roles = new SelectList(new[] { "staff", "sales", "importer" });
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView(user);
            }
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,Email,Password, Username,FullName,Role")] User user)
        {
            _logger.LogInformation($"Edit POST: UserId={id}, ModelState.IsValid={ModelState.IsValid}");

            if (id != user.UserId)
            {
                _logger.LogWarning($"ID mismatch: URL id={id}, user.UserId={user.UserId}");
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogError($"Validation error: {error.ErrorMessage}");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var userToUpdate = await _context.Users.FindAsync(id);
                    if (userToUpdate == null) return NotFound();

                    userToUpdate.FullName = user.FullName;
                    userToUpdate.Role = user.Role;

                    _context.Update(userToUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError(ex, "Lỗi xung đột khi cập nhật user {UserId}", id);
                    if (!_context.Users.Any(e => e.UserId == user.UserId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Roles = new SelectList(new[] { "staff", "sales", "importer" });

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView(user);
            }
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int UserId)
        {
            var user = await _context.Users.FindAsync(UserId);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSelected(List<int> selectedIds)
        {
            if (selectedIds == null || selectedIds.Count == 0)
            {
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var usersToRemove = await _context.Users
                                            .Where(u => selectedIds.Contains(u.UserId))
                                            .ToListAsync();

                if (usersToRemove.Any())
                {
                    _context.Users.RemoveRange(usersToRemove);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa nhiều tài khoản.");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
