using System;
using System.Collections.Generic;

using System.ComponentModel.DataAnnotations;

namespace RetailStore.Models;

public partial class Supplier
{
    public int SupplierId { get; set; }

    [Display(Name = "Tên nhà cung cấp")]
    [Required(ErrorMessage = "Vui lòng nhập tên nhà cung cấp")]
    [StringLength(100, ErrorMessage = "Tên không được vượt quá 100 ký tự")]
    public string Name { get; set; } = null!;

    [Display(Name = "Số điện thoại")]
    [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [StringLength(15, ErrorMessage = "Số điện thoại không quá 15 ký tự")]
    public string? Phone { get; set; }

    [Display(Name = "Email")]
    [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
    [StringLength(100)]
    public string? Email { get; set; }

    [Display(Name = "Địa chỉ")]
    [StringLength(200, ErrorMessage = "Địa chỉ không quá 200 ký tự")]
    public string? Address { get; set; }
}