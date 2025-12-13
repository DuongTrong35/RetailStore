using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace RetailStore.Models
{
    [Table("products")]
    public class Product
    {
        [Key]
        [Column("product_id")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        [StringLength(100)]
        [Display(Name = "Tên sản phẩm")]
        [Column("product_name")]
        public string ProductName { get; set; } = null!;

        [StringLength(50)]
        [Display(Name = "Mã vạch")]
        [Column("barcode")]
        public string? Barcode { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá bán")]
        [Column("price", TypeName = "decimal(10, 2)")]
        [Display(Name = "Giá bán")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá bán phải lớn hơn hoặc bằng 0")]
        public decimal Price { get; set; }

        [StringLength(20)]
        [Display(Name = "Đơn vị tính")]
        [Column("unit")]
        public string? Unit { get; set; }

        [Display(Name = "Danh mục")]
        [Column("category_id")]
        public int? CategoryId { get; set; }

        [Display(Name = "Nhà cung cấp")]
        [Column("supplier_id")]
        public int? SupplierId { get; set; }

        [Display(Name = "Ngày tạo")]
        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }


        [Display(Name = "Hình ảnh")]
        [Column("imageUrl")]
        public string? ImageUrl { get; set; }
        [NotMapped]
        [Display(Name = "Chọn hình ảnh")]
        public IFormFile? ImageUpload { get; set; }



        [ForeignKey("CategoryId")]
        [Display(Name = "Danh mục")]
        public virtual Category? Category { get; set; }

        [ForeignKey("SupplierId")]
        [Display(Name = "Nhà cung cấp")]
        public virtual Supplier? Supplier { get; set; }
    }
}