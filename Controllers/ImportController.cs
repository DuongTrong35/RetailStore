using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RetailStore.Models;

namespace RetailStore.Controllers
{
    public class ImportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ImportController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. QUẢN LÝ & TÌM KIẾM (Gộp chung vào GET)
        // GET: Import/Index
        public async Task<IActionResult> Index(string searchString, DateTime? fromDate, DateTime? toDate)
        {
            // Khởi tạo truy vấn
            var query = _context.ImportReceipts
                .Include(i => i.Supplier)
                .Include(i => i.User)
                .AsQueryable();

            // Logic lọc dữ liệu
            if (!string.IsNullOrEmpty(searchString))
            {
                // Lưu ý: Cần chuyển ID sang string để tìm kiếm
                query = query.Where(i =>
                       i.ImportId.ToString().Contains(searchString)              // 1. Tìm theo Mã phiếu
                    || i.Supplier.Name.Contains(searchString)                    // 2. Tìm theo Nhà cung cấp
                    || i.User.FullName.Contains(searchString)                    // 3. Tìm theo Người nhập
                    || i.ImportDetails.Any(d => d.Product.ProductName.Contains(searchString) // 4. Tìm theo Tên SP
                                             || d.Product.Barcode.Contains(searchString))    // 5. Tìm theo Barcode
                );
            }
            if (fromDate.HasValue)
            {
                query = query.Where(i => i.CreatedAt.Date >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                query = query.Where(i => i.CreatedAt.Date <= toDate.Value.Date);
            }

            // Sắp xếp và lấy dữ liệu
            var result = await query.OrderByDescending(i => i.CreatedAt).ToListAsync();

            // Lưu giữ lại giá trị filter để hiển thị trên View
            ViewData["CurrentFilter"] = searchString;
            ViewData["FromDate"] = fromDate?.ToString("yyyy-MM-dd");
            ViewData["ToDate"] = toDate?.ToString("yyyy-MM-dd");

            return View(result);
        }

        // 2. FORM TẠO MỚI
        // GET: Import/Create
        public IActionResult Create()
        {
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "SupplierId", "Name");
            ViewBag.Products = _context.Products.ToList();
            return View();
        }

        // 3. XỬ LÝ LƯU PHIẾU NHẬP (Phần bạn đang thiếu)
        // POST: Import/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int SupplierId, List<int> ProductIds, List<int> Quantities, List<decimal> ImportPrices)
        {
            // Sử dụng Transaction để đảm bảo an toàn dữ liệu (sai là rollback hết)
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // A. Tạo Header phiếu nhập
                    var importReceipt = new ImportReceipt
                    {
                        SupplierId = SupplierId,
                        UserId = 1, // Tạm fix ID user admin (hoặc lấy từ session)
                        CreatedAt = DateTime.Now,
                        TotalAmount = 0
                    };

                    _context.ImportReceipts.Add(importReceipt);
                    await _context.SaveChangesAsync(); // Lưu để lấy ID phiếu nhập

                    decimal grandTotal = 0;

                    // B. Tạo Chi tiết & Cập nhật Kho
                    if (ProductIds != null && ProductIds.Count > 0)
                    {
                        for (int i = 0; i < ProductIds.Count; i++)
                        {
                            int pId = ProductIds[i];
                            int qty = Quantities[i];
                            decimal price = ImportPrices[i];
                            decimal subtotal = qty * price;

                            // B1. Lưu chi tiết dòng hàng
                            var detail = new ImportDetail
                            {
                                ImportId = importReceipt.ImportId,
                                ProductId = pId,
                                Quantity = qty,
                                ImportPrice = price,
                                Subtotal = subtotal
                            };
                            _context.ImportDetails.Add(detail);
                            grandTotal += subtotal;

                            // B2. CỘNG TỒN KHO (Quan trọng)
                            var inventoryItem = await _context.Inventories
                                                    .FirstOrDefaultAsync(inv => inv.ProductId == pId);

                            if (inventoryItem != null)
                            {
                                inventoryItem.Quantity += qty;
                                inventoryItem.UpdatedAt = DateTime.Now;
                                _context.Inventories.Update(inventoryItem);
                            }
                            else
                            {
                                var newInventory = new Inventory
                                {
                                    ProductId = pId,
                                    Quantity = qty,
                                    UpdatedAt = DateTime.Now
                                };
                                _context.Inventories.Add(newInventory);
                            }
                        }
                    }

                    // C. Cập nhật lại tổng tiền cho phiếu nhập
                    importReceipt.TotalAmount = grandTotal;
                    _context.Update(importReceipt);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync(); // Xác nhận giao dịch thành công

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Nếu lỗi thì hoàn tác mọi thay đổi
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "Lỗi nhập hàng: " + ex.Message);

                    // Load lại dữ liệu để không bị trắng trang
                    ViewData["SupplierId"] = new SelectList(_context.Suppliers, "SupplierId", "Name", SupplierId);
                    ViewBag.Products = _context.Products.ToList();
                    return View();
                }
            }
        }

        // 4. XEM CHI TIẾT
        // GET: Import/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var importReceipt = await _context.ImportReceipts
                .Include(i => i.Supplier)
                .Include(i => i.User)
                .Include(i => i.ImportDetails)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(m => m.ImportId == id);

            if (importReceipt == null) return NotFound();

            return View(importReceipt);
        }
    }
}