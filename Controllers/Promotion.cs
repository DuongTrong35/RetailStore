using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailStore.Models;

namespace RetailStore.Controllers
{
	public class PromotionController : Controller
	{
		private readonly ApplicationDbContext _context;

		public PromotionController(ApplicationDbContext context)
		{
			_context = context;
		}

		// GET: Promotion
		public async Task<IActionResult> Index(string? searchString, string? status)
		{
			ViewData["CurrentFilter"] = searchString;
			ViewData["StatusFilter"] = status;

			var query = _context.Promotions.AsQueryable();

			if (!string.IsNullOrWhiteSpace(searchString))
			{
				query = query.Where(p => p.PromoCode.Contains(searchString) || (p.Description ?? string.Empty).Contains(searchString));
			}

			if (!string.IsNullOrWhiteSpace(status))
			{
				query = query.Where(p => p.Status == status);
			}

			var promotions = await query
				.OrderByDescending(p => p.StartDate)
				.ThenByDescending(p => p.PromoId)
				.ToListAsync();

			if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
			{
				return PartialView(promotions);
			}

			return View(promotions);
		}

		// GET: Promotion/Create
		public IActionResult Create()
		{
			var model = new Promotion
			{
				Status = "active",
				UsedCount = 0,
				StartDate = DateOnly.FromDateTime(DateTime.Today),
				EndDate = DateOnly.FromDateTime(DateTime.Today).AddDays(7),
				DiscountValue = 0,
				MinOrderAmount = 0
			};

			if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
			{
				return PartialView(model);
			}

			return View(model);
		}

		// POST: Promotion/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("PromoCode,Description,DiscountType,DiscountValue,StartDate,EndDate,MinOrderAmount,UsageLimit,Status")] Promotion promotion)
		{
			ValidatePromotionDates(promotion);
			ValidateDiscountValue(promotion);
			ValidateMinOrderAmount(promotion);
			ValidateUsageLimit(promotion);
			ValidateUsedCount(promotion);
			await ValidateDuplicateCode(promotion);

			if (ModelState.IsValid)
			{
				promotion.Status ??= "active";
				promotion.UsedCount = 0;

				_context.Add(promotion);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}

			if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
			{
				return PartialView(promotion);
			}

			return View(promotion);
		}

		// GET: Promotion/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var promotion = await _context.Promotions.FindAsync(id);
			if (promotion == null)
			{
				return NotFound();
			}

			if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
			{
				return PartialView(promotion);
			}

			return View(promotion);
		}

		// POST: Promotion/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("PromoId,PromoCode,Description,DiscountType,DiscountValue,StartDate,EndDate,MinOrderAmount,UsageLimit,UsedCount,Status")] Promotion promotion)
		{
			if (id != promotion.PromoId)
			{
				return NotFound();
			}

			ValidatePromotionDates(promotion);
			ValidateDiscountValue(promotion);
			ValidateMinOrderAmount(promotion);
			ValidateUsageLimit(promotion);
			ValidateUsedCount(promotion);
			await ValidateDuplicateCode(promotion);

			if (ModelState.IsValid)
			{
				try
				{
					var existing = await _context.Promotions.FindAsync(id);
					if (existing == null)
					{
						return NotFound();
					}

					existing.PromoCode = promotion.PromoCode;
					existing.Description = promotion.Description;
					existing.DiscountType = promotion.DiscountType;
					existing.DiscountValue = promotion.DiscountValue;
					existing.StartDate = promotion.StartDate;
					existing.EndDate = promotion.EndDate;
					existing.MinOrderAmount = promotion.MinOrderAmount;
					existing.UsageLimit = promotion.UsageLimit;
					existing.UsedCount = promotion.UsedCount;
					existing.Status = promotion.Status;

					_context.Update(existing);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!PromotionExists(promotion.PromoId))
					{
						return NotFound();
					}
					throw;
				}

				return RedirectToAction(nameof(Index));
			}

			if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
			{
				return PartialView(promotion);
			}

			return View(promotion);
		}

		// POST: Promotion/Delete
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int promoId)
		{
			var promotion = await _context.Promotions.FindAsync(promoId);
			if (promotion != null)
			{
				_context.Promotions.Remove(promotion);
				await _context.SaveChangesAsync();
			}
			return RedirectToAction(nameof(Index));
		}

		// POST: Promotion/DeleteSelected
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteSelected(List<int> selectedIds)
		{
			if (selectedIds == null || selectedIds.Count == 0)
			{
				return RedirectToAction(nameof(Index));
			}

			var promotionsToRemove = await _context.Promotions
				.Where(p => selectedIds.Contains(p.PromoId))
				.ToListAsync();

			if (promotionsToRemove.Any())
			{
				_context.Promotions.RemoveRange(promotionsToRemove);
				await _context.SaveChangesAsync();
			}

			return RedirectToAction(nameof(Index));
		}

		private bool PromotionExists(int id)
		{
			return _context.Promotions.Any(e => e.PromoId == id);
		}

		private void ValidatePromotionDates(Promotion promotion)
		{
			if (promotion.EndDate < promotion.StartDate)
			{
				ModelState.AddModelError(nameof(Promotion.EndDate), "Ngày kết thúc phải sau hoặc bằng ngày bắt đầu.");
			}
		}

		private void ValidateDiscountValue(Promotion promotion)
		{
			if (promotion.DiscountValue <= 0)
			{
				ModelState.AddModelError(nameof(Promotion.DiscountValue), "Giá trị giảm phải lớn hơn 0.");
			}

			if ((promotion.DiscountType == "percent" || promotion.DiscountType == "Phần trăm") && promotion.DiscountValue > 100)
			{
				ModelState.AddModelError(nameof(Promotion.DiscountValue), "Giảm theo phần trăm không được vượt quá 100%.");
			}

			if ((promotion.DiscountType == "percent" || promotion.DiscountType == "Phần trăm") && promotion.DiscountValue < 1)
			{
				ModelState.AddModelError(nameof(Promotion.DiscountValue), "Giảm theo phần trăm phải trong phạm vi từ 1% đến 100%.");
			}
		}

		private void ValidateMinOrderAmount(Promotion promotion)
		{
			if (promotion.MinOrderAmount.HasValue && promotion.MinOrderAmount.Value < 0)
			{
				ModelState.AddModelError(nameof(Promotion.MinOrderAmount), "Giá trị đơn hàng tối thiểu phải ≥ 0.");
			}
		}

		private void ValidateUsageLimit(Promotion promotion)
		{
			if (promotion.UsageLimit.HasValue && promotion.UsageLimit.Value < 1)
			{
				ModelState.AddModelError(nameof(Promotion.UsageLimit), "Giới hạn sử dụng phải ≥ 1.");
			}
		}

		private void ValidateUsedCount(Promotion promotion)
		{
			if (promotion.UsedCount.HasValue && promotion.UsedCount.Value < 0)
			{
				ModelState.AddModelError(nameof(Promotion.UsedCount), "Số lần đã sử dụng phải ≥ 0.");
			}

			if (promotion.UsageLimit.HasValue && promotion.UsedCount.HasValue && promotion.UsedCount.Value > promotion.UsageLimit.Value)
			{
				ModelState.AddModelError(nameof(Promotion.UsedCount), "Số lần đã dùng không thể lớn hơn giới hạn sử dụng.");
			}
		}

		private async Task ValidateDuplicateCode(Promotion promotion)
		{
			var exists = await _context.Promotions
				.AnyAsync(p => p.PromoCode == promotion.PromoCode && p.PromoId != promotion.PromoId);
			if (exists)
			{
				ModelState.AddModelError(nameof(Promotion.PromoCode), "Mã khuyến mãi đã tồn tại.");
			}
		}
	}
}
