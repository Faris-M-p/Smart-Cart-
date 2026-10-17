using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCartPro.Entities;

/// <summary>Maps to <c>modules</c>. Platform-wide module catalog.</summary>
[Table("modules")]
public class Module
{
    [Key]
    [Column("id_module")]
    public int IdModule { get; set; }

    [Required]
    [Column("modulecode")]
    public string ModuleCode { get; set; } = string.Empty;

    [Required]
    [Column("modulename")]
    public string ModuleName { get; set; } = string.Empty;

    [Column("displayorder")]
    public int DisplayOrder { get; set; }

    [Column("isactive")]
    public bool IsActive { get; set; } = true;

    [Column("createdat")]
    public DateTime CreatedAt { get; set; }

    [Column("updatedat")]
    public DateTime? UpdatedAt { get; set; }

    [Column("cancelled")]
    public bool Cancelled { get; set; }

    [Column("cancelledon")]
    public DateTime? CancelledOn { get; set; }

    [Column("cancelledreason")]
    public string? CancelledReason { get; set; }
}

/// <summary>Maps to <c>module_presets</c>. Basic / Premium preset definitions.</summary>
[Table("module_presets")]
public class ModulePreset
{
    [Key]
    [Column("id_preset")]
    public int IdPreset { get; set; }

    [Required]
    [Column("presetcode")]
    public string PresetCode { get; set; } = string.Empty;

    [Required]
    [Column("presetname")]
    public string PresetName { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("displayorder")]
    public int DisplayOrder { get; set; }

    [Column("createdat")]
    public DateTime CreatedAt { get; set; }

    public ICollection<ModulePresetItem> Items { get; set; } = new List<ModulePresetItem>();
}

/// <summary>Maps to <c>module_preset_items</c>. Module ↔ preset join.</summary>
[Table("module_preset_items")]
public class ModulePresetItem
{
    [Key]
    [Column("id_preset_item")]
    public int IdPresetItem { get; set; }

    [Column("fk_preset")]
    public int FkPreset { get; set; }

    [Column("fk_module")]
    public int FkModule { get; set; }

    [ForeignKey(nameof(FkPreset))]
    public ModulePreset? Preset { get; set; }

    [ForeignKey(nameof(FkModule))]
    public Module? Module { get; set; }
}

/// <summary>Maps to <c>installation_modules</c>. The final enabled module list for this installation.</summary>
[Table("installation_modules")]
public class InstallationModule
{
    [Key]
    [Column("id_inst_module")]
    public int IdInstModule { get; set; }

    [Column("fk_module")]
    public int FkModule { get; set; }

    [Column("createdat")]
    public DateTime CreatedAt { get; set; }

    [ForeignKey(nameof(FkModule))]
    public Module? Module { get; set; }
}
