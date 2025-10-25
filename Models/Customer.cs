using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RetailStore.Models;

public partial class Customer
{
    public int CustomerId { get; set; }

    [Required(ErrorMessage = "Tên khách hàng là bắt buộc.")]
    [StringLength(100, ErrorMessage = "Tên khách hàng không được vượt quá 100 ký tự.")]
    public string Name { get; set; } = null!;

    [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự.")]
    public string? Phone { get; set; }

    [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự.")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    public string? Email { get; set; }

    [StringLength(500, ErrorMessage = "Địa chỉ không được vượt quá 500 ký tự.")]
    public string? Address { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Điểm tích lũy phải là số dương.")]
    public int LoyaltyPoints { get; set; } = 0;

    [StringLength(50, ErrorMessage = "Phân hạng không được vượt quá 50 ký tự.")]
    public string? CustomerTier { get; set; } = "Thành viên";

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }
}
