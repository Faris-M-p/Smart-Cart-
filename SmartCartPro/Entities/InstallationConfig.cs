using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCartPro.Entities;

/// <summary>Maps to <c>installation_config</c>. Tracks wizard progress, theme, store mode.</summary>
[Table("installation_config")]
public class InstallationConfig
{
    [Key]
    [Column("id_config")]
    public int IdConfig { get; set; }

    [Column("fk_license")]
    public int? FkLicense { get; set; }

    /// <summary>light | dark</summary>
    [Required]
    [Column("theme")]
    public string Theme { get; set; } = "light";

    /// <summary>single | multi</summary>
    [Required]
    [Column("store_mode")]
    public string StoreMode { get; set; } = "single";

    /// <summary>Last completed wizard step (0 = not started, 8 = all done).</summary>
    [Column("onboarding_step")]
    public int OnboardingStep { get; set; }

    [Column("onboarding_complete")]
    public bool OnboardingComplete { get; set; }

    [Column("activated_at")]
    public DateTime? ActivatedAt { get; set; }

    [Column("createdat")]
    public DateTime CreatedAt { get; set; }

    [Column("updatedat")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey(nameof(FkLicense))]
    public License? License { get; set; }
}
