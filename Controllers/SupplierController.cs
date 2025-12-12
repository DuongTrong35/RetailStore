using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailStore.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.Logging;

namespace RetailStore.Controllers
{
    public class SupplierController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SupplierController> _logger;

        public SupplierController(ApplicationDbContext context, ILogger<SupplierController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Supplier
        public async Task<IActionResult> Index(string sortOrder, string searchString)
        {
            try
            {
                ViewData["CurrentSort"] = sortOrder;
                ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
                ViewData["PhoneSortParm"] = sortOrder == "Phone" ? "phone_desc" : "Phone";
                ViewData["EmailSortParm"] = sortOrder == "Email" ? "email_desc" : "Email";
                ViewData["AddressSortParm"] = sortOrder == "Address" ? "address_desc" : "Address";

                ViewData["CurrentFilter"] = searchString;

                var suppliersQuery = _context.Suppliers.AsQueryable();

                if (!String.IsNullOrEmpty(searchString))
                {
                    suppliersQuery = suppliersQuery.Where(s =>
                        s.Name.Contains(searchString) ||
                        s.Phone.Contains(searchString) ||
                        s.Email.Contains(searchString) ||
                        s.Address.Contains(searchString)
                    );
                }

                switch (sortOrder)
                {
                    case "name_desc":
                        suppliersQuery = suppliersQuery.OrderByDescending(s => s.Name);
                        break;
                    case "Phone":
                        suppliersQuery = suppliersQuery.OrderBy(s => s.Phone);
                        break;
                    case "phone_desc":
                        suppliersQuery = suppliersQuery.OrderByDescending(s => s.Phone);
                        break;
                    case "Email":
                        suppliersQuery = suppliersQuery.OrderBy(s => s.Email);
                        break;
                    case "email_desc":
                        suppliersQuery = suppliersQuery.OrderByDescending(s => s.Email);
                        break;
                    case "Address":
                        suppliersQuery = suppliersQuery.OrderBy(s => s.Address);
                        break;
                    case "address_desc":
                        suppliersQuery = suppliersQuery.OrderByDescending(s => s.Address);
                        break;
                    default:
                        suppliersQuery = suppliersQuery.OrderBy(s => s.Name);
                        break;
                }

                var suppliers = await suppliersQuery.ToListAsync();

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView(suppliers);
                }

                return View(suppliers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách nhà cung cấp.");
                return View("Error");
            }
        }

        // GET: Supplier/Create
        public IActionResult Create()
        {
            var model = new Supplier();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView(model);
            }
            return View(model);
        }

        // POST: Supplier/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Phone,Email,Address")] Supplier supplier)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(supplier);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Thêm nhà cung cấp thành công!";

                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo nhà cung cấp mới.");
                ModelState.AddModelError("", "Không thể lưu nhà cung cấp, vui lòng thử lại.");
                TempData["ErrorMessage"] = "Có lỗi xảy ra, vui lòng thử lại.";  
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView(supplier);
            }
            return View(supplier);
        }

        // GET: Supplier/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
            {
                return NotFound();
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView(supplier);
            }
            return View(supplier);
        }

        // POST: Supplier/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SupplierId,Name,Phone,Email,Address")] Supplier supplier)
        {
            if (id != supplier.SupplierId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var supplierToUpdate = await _context.Suppliers.FindAsync(id);
                    if (supplierToUpdate == null) return NotFound();

                    supplierToUpdate.Name = supplier.Name;
                    supplierToUpdate.Phone = supplier.Phone;
                    supplierToUpdate.Email = supplier.Email;
                    supplierToUpdate.Address = supplier.Address;

                    _context.Update(supplierToUpdate);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Sửa nhà cung cấp thành công!";
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError(ex, "Lỗi xung đột khi cập nhật nhà cung cấp {SupplierId}", id);
                    if (!_context.Suppliers.Any(e => e.SupplierId == supplier.SupplierId))
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

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView(supplier);
            }
            return View(supplier);
        }

        // POST: Supplier/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int SupplierId)
        {
            var supplier = await _context.Suppliers.FindAsync(SupplierId);
            if (supplier != null)
            {
                _context.Suppliers.Remove(supplier);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa nhà cung cấp thành công!";
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
                var suppliersToRemove = await _context.Suppliers
                                                    .Where(s => selectedIds.Contains(s.SupplierId))
                                                    .ToListAsync();

                if (suppliersToRemove.Any())
                {
                    _context.Suppliers.RemoveRange(suppliersToRemove);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa nhiều nhà cung cấp.");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}