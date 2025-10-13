using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RetailStore.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Tên danh mục là bắt buộc.")]
    [StringLength(100)]
    public string CategoryName { get; set; } = null!;
}
