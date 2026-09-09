using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    /// <summary>Maps to Database-Postgres/01_Tables/modules.sql (modules).</summary>
    [Table("modules")]
    public class ModuleEntity
    {
        [Key]
        [Column("id_module")]
        public int ID_Module { get; set; }

        [MaxLength(100)]
        [Column("modulename")]
        public string ModuleName { get; set; } = string.Empty;

        [MaxLength(150)]
        [Column("displayname")]
        public string DisplayName { get; set; } = string.Empty;

        [Column("displayorder")]
        public int DisplayOrder { get; set; }

        [Column("isactive")]
        public bool IsActive { get; set; }

        [Column("createdat")]
        public DateTime CreatedAt { get; set; }

        [Column("updatedat")]
        public DateTime? UpdatedAt { get; set; }

        [Column("cancelled")]
        public bool Cancelled { get; set; }

        [Column("cancelledon")]
        public DateTime? CancelledOn { get; set; }

        [MaxLength(255)]
        [Column("cancelledreason")]
        public string? CancelledReason { get; set; }
    }
}
