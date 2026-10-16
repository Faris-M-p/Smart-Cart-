using SmartCartPro.Models;

namespace SmartCartPro.Services.Interfaces;

public interface IAuthService
{
    Task<AuthSessionResult> LoginAsync(LoginInput input, CancellationToken cancellationToken = default);

    Task<AuthSessionResult> VerifyOtpAsync(VerifyOtpInput input, CancellationToken cancellationToken = default);

    Task<AuthSessionResult> ResendOtpAsync(string challengeToken, CancellationToken cancellationToken = default);

    Task<AuthSessionResult> ChangePasswordAsync(
        int idUser,
        ChangePasswordInput input,
        CancellationToken cancellationToken = default);

    Task<AuthSessionResult> SwitchStoreAsync(
        int idUser,
        int fkStore,
        bool rememberMe,
        CancellationToken cancellationToken = default);

    void SetAuthCookie(string token, DateTime? expiresAt, bool rememberMe);

    void ClearAuthCookie();
}
