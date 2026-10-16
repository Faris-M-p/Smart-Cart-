namespace SmartCartPro.Entities;

/// <summary>Maps to <c>auth_otp_challenges</c> — Super Admin email OTP.</summary>
public class AuthOtpChallenge
{
    public int IdAuthOtpChallenge { get; set; }
    public string ChallengeToken { get; set; } = string.Empty;
    public int FkUser { get; set; }
    public string OtpHash { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
    public int AttemptCount { get; set; }
    public int MaxAttempts { get; set; } = 5;
    public DateTime ExpiresAt { get; set; }
    public DateTime? ConsumedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public User? User { get; set; }
}
