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
        public async Task<IActionResult> Index()
        {
            try
            {
                var categories = await _context.Categories.ToListAsync();
                return View(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Không thể lấy danh sách categories.");
                return View("Error");
            }
        }
    }
}
