using SmartCartPro.Models;

namespace SmartCartPro.Services.Interfaces;

public interface IAuthService
{
    Task<OutputAuthSession> LoginAsync(InputAuthLogin input, CancellationToken cancellationToken = default);

    Task<OutputAuthSession> VerifyOtpAsync(InputAuthVerifyOtp input, CancellationToken cancellationToken = default);

    Task<OutputAuthSession> ResendOtpAsync(InputAuthResendOtp input, CancellationToken cancellationToken = default);

    Task<OutputAuthSession> ChangePasswordAsync(
        InputAuthChangePassword input,
        CancellationToken cancellationToken = default);

    /// <summary>Emails a time-limited reset link. Never reveals whether the account exists.</summary>
    Task<OutputAuthForgotPassword> RequestPasswordResetAsync(
        InputAuthForgotPassword input,
        CancellationToken cancellationToken = default);

    /// <summary>Checks that a reset token is genuine, unexpired and not yet used.</summary>
    Task<OutputAuthResetTokenSelect> GetPasswordResetTokenAsync(
        InputAuthResetTokenSelect input,
        CancellationToken cancellationToken = default);

    /// <summary>Sets a new password from a valid reset token (single use).</summary>
    Task<OutputAuthResetPassword> ResetPasswordAsync(
        InputAuthResetPassword input,
        CancellationToken cancellationToken = default);

    Task<OutputAuthSession> SwitchStoreAsync(
        int idUser,
        int fkStore,
        bool rememberMe,
        CancellationToken cancellationToken = default);

    void SetAuthCookie(string token, DateTime? expiresAt, bool rememberMe);

    void ClearAuthCookie();
}
