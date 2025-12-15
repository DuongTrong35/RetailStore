using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailStore.Models;

namespace RetailStore.Controllers
{
    public class InventoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CategoryController> _logger;

        public InventoryController(ApplicationDbContext context, ILogger<CategoryController> logger)
        {
            _context = context;
            _logger = logger;
        }
        //public IActionResult Index()
        //{

        //    return View();
        //}

        public async Task<IActionResult> Index()
        {
            try
            {
                var inventories = await _context.Inventories.ToListAsync();

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView(inventories);
                }

                return View(inventories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Không thể lấy danh sách inventory.");
                return View("Error");
            }
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Inventory inventory)
        {
            if (ModelState.IsValid)
            {
                inventory.UpdatedAt = DateTime.Now;

                _context.Inventories.Add(inventory);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(inventory);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var inventory = await _context.Inventories.FindAsync(id);
            if (inventory == null)
                return NotFound();

            return View(inventory);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Inventory inventory)
        {
            if (id != inventory.InventoryId)
                return NotFound();

            if (ModelState.IsValid)
            {
                inventory.UpdatedAt = DateTime.Now;
                _context.Update(inventory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(inventory);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSelected(int[] selectedIds)
        {
            if (selectedIds == null || selectedIds.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn ít nhất một mục để xóa.";
                return RedirectToAction(nameof(Index));
            }

            var inventories = await _context.Inventories
                .Where(i => selectedIds.Contains(i.InventoryId))
                .ToListAsync();

            if (inventories.Any())
            {
                _context.Inventories.RemoveRange(inventories);
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = $"Đã xóa {inventories.Count} mục tồn kho.";
            return RedirectToAction(nameof(Index));
        }

    }
}
