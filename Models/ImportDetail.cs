using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RetailStore.Models;

[Table("import_details")]
public class ImportDetail
{
    [Key]
    [Column("import_detail_id")]
    public int ImportDetailId { get; set; }

    [Column("import_id")]
    public int ImportId { get; set; }

    [ForeignKey("ImportId")]
    public virtual ImportReceipt? ImportReceipt { get; set; }

    [Column("product_id")]
    public int ProductId { get; set; }

    public virtual Product? Product { get; set; }

    [Column("quantity")]
    public int Quantity { get; set; }

    [Column("import_price")]
    public decimal ImportPrice { get; set; }

    [Column("subtotal")]
    public decimal Subtotal { get; set; }
}