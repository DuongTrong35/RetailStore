using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RetailStore.Models
{
    [Table("categories")]
    public class Category
    {
        [Key]
        [Column("category_id")]
        public int CategoryId { get; set; }

        [Column("category_name")]
        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên danh mục tối đa 100 ký tự")]
        [Display(Name = "Tên danh mục")]
        public string CategoryName { get; set; } = null!;

    }
}