using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCartPro.Entities;

/// <summary>Maps to <c>auth_otp_challenges</c> — Super Admin email OTP.</summary>
[Table("auth_otp_challenges")]
public class AuthOtpChallenge
{
    [Key]
    [Column("id_authotpchallenge")]
    public int IdAuthOtpChallenge { get; set; }

    [Required]
    [Column("challengetoken")]
    public string ChallengeToken { get; set; } = string.Empty;

    [Column("fk_user")]
    public int FkUser { get; set; }

    [Required]
    [Column("otphash")]
    public string OtpHash { get; set; } = string.Empty;

    [Column("rememberme")]
    public bool RememberMe { get; set; }

    [Column("attemptcount")]
    public int AttemptCount { get; set; }

    [Column("maxattempts")]
    public int MaxAttempts { get; set; } = 5;

    [Column("expiresat")]
    public DateTime ExpiresAt { get; set; }

    [Column("consumedat")]
    public DateTime? ConsumedAt { get; set; }

    [Column("createdat")]
    public DateTime CreatedAt { get; set; }

    [ForeignKey(nameof(FkUser))]
    public User? User { get; set; }
}

