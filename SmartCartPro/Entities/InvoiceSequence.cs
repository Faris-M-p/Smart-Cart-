using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCartPro.Entities;

/// <summary>Maps to <c>invoice_sequences</c>. Store-scoped document number sequences.</summary>
[Table("invoice_sequences")]
public class InvoiceSequence
{
    [Key]
    [Column("id_invoicesequence")]
    public int IdInvoiceSequence { get; set; }

    [Column("fk_store")]
    public int FkStore { get; set; }

    [Required]
    [Column("sequencetype")]
    public string SequenceType { get; set; } = string.Empty;

    [Column("prefix")]
    public string Prefix { get; set; } = string.Empty;

    [Column("nextnumber")]
    public int NextNumber { get; set; } = 1;

    [Column("padding")]
    public int Padding { get; set; } = 6;

    [Column("createdat")]
    public DateTime CreatedAt { get; set; }

    [Column("updatedat")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey(nameof(FkStore))]
    public Store? Store { get; set; }
}
