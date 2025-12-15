using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RetailStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RetailStore.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(ApplicationDbContext context, ILogger<AccountController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string searchString, string selectedRole, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                ViewData["CurrentFilter"] = searchString;
                ViewData["SelectedRole"] = selectedRole;
                ViewData["FromDate"] = fromDate?.ToString("yyyy-MM-dd"); // Định dạng cho input date
                ViewData["ToDate"] = toDate?.ToString("yyyy-MM-dd"); // Định dạng cho input date

                var roles = await _context.Users.Select(u => u.Role).Distinct().ToListAsync();
                ViewBag.Roles = roles;

                var usersQuery = _context.Users.AsQueryable();

                if (!String.IsNullOrEmpty(searchString))
                {
                    usersQuery = usersQuery.Where(u =>
                        u.Username.Contains(searchString) ||
                        u.FullName.Contains(searchString)
                    );
                }

                if (!string.IsNullOrEmpty(selectedRole))
                {
                    usersQuery = usersQuery.Where(u => u.Role == selectedRole);
                }

                if (fromDate.HasValue)
                {
                    usersQuery = usersQuery.Where(u => u.CreatedAt >= fromDate.Value);
                }
                if (toDate.HasValue)
                {
                    // Lấy đến cuối ngày đó (23:59:59) để đảm bảo chính xác
                    var endOfDate = toDate.Value.Date.AddDays(1).AddTicks(-1);
                    usersQuery = usersQuery.Where(u => u.CreatedAt <= endOfDate);
                }

                var users = await usersQuery.ToListAsync();

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView(users);
                }

                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách tài khoản.");
                return View("Error");
            }
        }

        public async Task<bool> IsUsernameExistsAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.Username == username);
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> IsUsernameExistsAsyncForUpdate(string username, int? excludeUserId = null)
        {
            return await _context.Users
                .AnyAsync(u => u.Username == username && u.UserId != excludeUserId);
        }

        public async Task<bool> IsEmailExistsAsyncForUpdate(string email, int? excludeUserId = null)
        {
            return await _context.Users
                .AnyAsync(u => u.Email == email && u.UserId != excludeUserId);
        }

        public IActionResult Create()
        {
            var model = new User();

            ViewBag.Roles = new SelectList(new[] { "admin", "staff", "sales", "importer" });

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
                if (await IsUsernameExistsAsync(user.Username))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
                }

                if (await IsEmailExistsAsync(user.Email))
                {
                    ModelState.AddModelError("Email", "Email đã tồn tại");
                }
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

            ViewBag.Roles = new SelectList(new[] { "admin", "staff", "sales", "importer" });

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
            ViewBag.Roles = new SelectList(new[] { "admin", "staff", "sales", "importer" });
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

            if (await IsUsernameExistsAsyncForUpdate(user.Username, id))
            {
                ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
            }

            if (await IsEmailExistsAsyncForUpdate(user.Email, id))
            {
                ModelState.AddModelError("Email", "Email đã tồn tại");
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

            ViewBag.Roles = new SelectList(new[] { "admin", "staff", "sales", "importer" });

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