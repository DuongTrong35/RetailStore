using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RetailStore.Models
{
    [Table("suppliers")]
    public class Supplier
    {
        [Key]
        [Column("supplier_id")]
        public int SupplierId { get; set; }

        [Column("name")]
        [Required(ErrorMessage = "Tên nhà cung cấp là bắt buộc")]
        [StringLength(100)]
        [Display(Name = "Tên nhà cung cấp")]
        public string Name { get; set; } = null!;

        [Column("phone")]
        [StringLength(20)]
        [Display(Name = "Số điện thoại")]
        public string? Phone { get; set; }

        [Column("email")]
        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Column("address")]
        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

    }
}