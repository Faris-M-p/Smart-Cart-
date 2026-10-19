using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCartPro.Entities;

/// <summary>
/// Platform / company / store configuration key-value pairs. Maps to <c>settings</c>.
/// <para>setting_scope: 'platform' | 'company' | 'store'</para>
/// </summary>
[Table("settings")]
public class Setting
{
    [Key]
    [Column("id_setting")]
    public int IdSetting { get; set; }

    [Required]
    [Column("setting_scope")]
    public string SettingScope { get; set; } = "company";

    [Column("fk_store")]
    public int? FkStore { get; set; }

    [Required]
    [Column("settingkey")]
    public string SettingKey { get; set; } = string.Empty;

    [Column("settingvalue")]
    public string? SettingValue { get; set; }

    [Required]
    [Column("valuetype")]
    public string ValueType { get; set; } = "string";

    [Column("description")]
    public string? Description { get; set; }

    [Column("createdat")]
    public DateTime CreatedAt { get; set; }

    [Column("updatedat")]
    public DateTime? UpdatedAt { get; set; }
}
