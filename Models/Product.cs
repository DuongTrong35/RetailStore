using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RetailStore.Models
{

    [Table("products")]
    public class Product
    {
        [Key]
        [Column("product_id")]
        public int ProductId { get; set; }

        [Column("category_id")]
        public int? CategoryId { get; set; }

        [Column("supplier_id")]
        public int? SupplierId { get; set; }

        [Column("product_name")]
        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên sản phẩm không quá 100 ký tự")]
        [Display(Name = "Tên sản phẩm")]
        public string ProductName { get; set; } = null!;

        [Column("barcode")]
        [StringLength(50)]
        [Display(Name = "Mã vạch")]
        public string? Barcode { get; set; }

        [Column("price")]
        [Display(Name = "Giá bán")]
        [Range(0, 99999999999, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0")]
        [DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = true)]
        public decimal Price { get; set; }

        [Column("unit")]
        [StringLength(20)]
        [Display(Name = "Đơn vị tính")]
        public string? Unit { get; set; } = "pcs";

        [Column("created_at")]
        [Display(Name = "Ngày tạo")]
        public DateTime? CreatedAt { get; set; }


        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        [ForeignKey("SupplierId")]
        public virtual Supplier? Supplier { get; set; }
    }
}