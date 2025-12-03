using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RetailStore.Models;
[Table("users")]
public partial class User
{
    [Column("user_id")]
    public int UserId { get; set; }

    [Column("username")]
    [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")] // Bắt buộc nhập
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên đăng nhập từ 3 đến 50 ký tự")]
    public string Username { get; set; } = null!;

    [Column("password")]
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    [MinLength(6, ErrorMessage = "Mật khẩu phải ít nhất 6 ký tự")]
    public string Password { get; set; } = null!;

    [Column("full_name")]
    [Display(Name = "Họ và tên")]
    [Required(ErrorMessage = "Vui lòng nhập họ tên")]
    public string? FullName { get; set; }

    [Column("role")]
    [Required(ErrorMessage = "Vui lòng chọn vai trò")]
    public string? Role { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }
}
