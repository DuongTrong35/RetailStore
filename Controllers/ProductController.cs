using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RetailStore.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;

using Microsoft.AspNetCore.Hosting;

namespace RetailStore.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(string searchString, int? categoryId, int? supplierId)
        {

            var products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.ProductName.Contains(searchString) ||
                                               (p.Barcode != null && p.Barcode.Contains(searchString)));
            }
            if (categoryId.HasValue) products = products.Where(p => p.CategoryId == categoryId);
            if (supplierId.HasValue) products = products.Where(p => p.SupplierId == supplierId);

            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", categoryId);
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "SupplierId", "Name", supplierId);
            ViewData["CurrentFilter"] = searchString;

            var result = await products.OrderByDescending(p => p.ProductId).ToListAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest") return PartialView(result);
            return View(result);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName");
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "SupplierId", "Name");
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Create([Bind("ProductId,ProductName,Barcode,Price,Unit,CategoryId,SupplierId,ImageUpload")] Product product)
        {
            if (ModelState.IsValid)
            {
                product.CreatedAt = DateTime.Now;


                if (product.ImageUpload != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/products");
                    // Tạo thư mục nếu chưa có
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await product.ImageUpload.CopyToAsync(fileStream);
                    }

                    product.ImageUrl = "/images/products/" + uniqueFileName;
                }


                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "SupplierId", "Name", product.SupplierId);
            return View(product);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "SupplierId", "Name", product.SupplierId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,ProductName,Barcode,Price,Unit,CategoryId,SupplierId,CreatedAt,ImageUrl,ImageUpload")] Product product)
        {
            if (id != product.ProductId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {

                    if (product.ImageUpload != null)
                    {
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/products");
                        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await product.ImageUpload.CopyToAsync(fileStream);
                        }


                        product.ImageUrl = "/images/products/" + uniqueFileName;
                    }


                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "SupplierId", "Name", product.SupplierId);
            return View(product);
        }


        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                Console.WriteLine("Soft deleting product with ID: " + id);
                product.status="deleted";
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}