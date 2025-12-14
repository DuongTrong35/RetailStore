using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RetailStore.Models;

[Table("import_receipts")]
public class ImportReceipt
{
    [Key]
    [Column("import_id")]
    public int ImportId { get; set; }

    [Column("supplier_id")]
    public int SupplierId { get; set; }

    [ForeignKey("SupplierId")]
    public virtual Supplier? Supplier { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [ForeignKey("UserId")]
    public virtual User? User { get; set; }

    [Column("total_amount")]
    public decimal TotalAmount { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public virtual ICollection<ImportDetail> ImportDetails { get; set; } = new List<ImportDetail>();
}