using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RetailStore.Models;

public partial class Promotion
{
    public int PromoId { get; set; }

    [Required(ErrorMessage = "Mã khuyến mãi không được để trống.")]
    [StringLength(50, ErrorMessage = "Mã khuyến mãi không được vượt quá 50 ký tự.")]
    public string PromoCode { get; set; } = null!;

    [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Loại giảm giá không được để trống.")]
    [RegularExpression("percent|fixed|Phần trăm|Cố định",
        ErrorMessage = "Loại giảm giá phải là 'percent'/'fixed' hoặc 'Phần trăm'/'Cố định'.")]
    public string DiscountType { get; set; } = null!;

    [Required(ErrorMessage = "Giá trị giảm giá không được để trống.")]
    public decimal DiscountValue { get; set; }

    [Required(ErrorMessage = "Ngày bắt đầu không được bỏ trống.")]
    public DateOnly StartDate { get; set; }

    [Required(ErrorMessage = "Ngày kết thúc không được bỏ trống.")]
    public DateOnly EndDate { get; set; }

    public decimal? MinOrderAmount { get; set; }

    public int? UsageLimit { get; set; }

    public int? UsedCount { get; set; }

    public string? Status { get; set; }

    // Điều kiện
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Kiểm tra ngày
        if (EndDate < StartDate)
        {
            yield return new ValidationResult(
                "Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.",
                new[] { nameof(EndDate) });
        }

        // Nếu là giảm phần trăm thì không quá 100%
        if ((DiscountType == "Percentage" || DiscountType == "percent") && DiscountValue > 100)
        {
            yield return new ValidationResult(
                "Giảm theo phần trăm không được vượt quá 100%.",
                new[] { nameof(DiscountValue) });
        }

        // Nếu là giảm phần trăm thì phải từ 1% đến 100%
        if ((DiscountType == "Percentage" || DiscountType == "percent") && DiscountValue < 1)
        {
            yield return new ValidationResult(
                "Giảm theo phần trăm phải trong phạm vi từ 1% đến 100%.",
                new[] { nameof(DiscountValue) });
        }

        // UsedCount không được vượt quá UsageLimit
        if (UsageLimit.HasValue && UsedCount.HasValue && UsedCount > UsageLimit)
        {
            yield return new ValidationResult(
                "Số lần đã dùng không thể lớn hơn giới hạn sử dụng.",
                new[] { nameof(UsedCount) });
        }
    }
}
