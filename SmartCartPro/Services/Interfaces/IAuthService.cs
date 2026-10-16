using SmartCartPro.Models;

namespace SmartCartPro.Services.Interfaces;

public interface IAuthService
{
    Task<AuthSessionResult> LoginAsync(LoginInput input, CancellationToken cancellationToken = default);

    Task<AuthSessionResult> ChangePasswordAsync(
        int idUser,
        ChangePasswordInput input,
        CancellationToken cancellationToken = default);

    Task<AuthSessionResult> SwitchStoreAsync(
        int idUser,
        int fkStore,
        bool rememberMe,
        CancellationToken cancellationToken = default);

    /// <summary>Writes the JWT HttpOnly auth cookie for the current response.</summary>
    void SetAuthCookie(string token, DateTime? expiresAt, bool rememberMe);

    /// <summary>Deletes the JWT auth cookie for the current response.</summary>
    void ClearAuthCookie();
}
