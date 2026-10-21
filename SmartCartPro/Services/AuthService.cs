using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SmartCartPro.Configuration;
using SmartCartPro.Entities;
using SmartCartPro.Exceptions;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Services;

public class AuthService : IAuthService
{
    private const string ResetTokenPurpose = "SmartCartPro.Auth.PasswordReset.v1";
    private const string ResetLinkInvalidMessage =
        "This password reset link is invalid or has expired. Request a new one.";
    private const string ForgotPasswordGenericMessage =
        "If an account matches what you entered, a password reset link has been sent to its email address.";

    private readonly IAuthRepository _authRepository;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly AuthJwtTokenService _jwtTokenService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEmailService _emailService;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly OtpSettings _otpSettings;
    private readonly EnvironmentSettings _environmentSettings;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IAuthRepository authRepository,
        IPasswordHasher<User> passwordHasher,
        AuthJwtTokenService jwtTokenService,
        IHttpContextAccessor httpContextAccessor,
        IEmailService emailService,
        IDataProtectionProvider dataProtectionProvider,
        IHostEnvironment hostEnvironment,
        IOptions<OtpSettings> otpSettings,
        IOptions<EnvironmentSettings> environmentSettings,
        ILogger<AuthService> logger)
    {
        _authRepository = authRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _httpContextAccessor = httpContextAccessor;
        _emailService = emailService;
        _dataProtectionProvider = dataProtectionProvider;
        _hostEnvironment = hostEnvironment;
        _otpSettings = otpSettings.Value;
        _environmentSettings = environmentSettings.Value;
        _logger = logger;
    }

    public async Task<OutputAuthSession> LoginAsync(
        InputAuthLogin input,
        CancellationToken cancellationToken = default)
    {
        var user = await _authRepository.FindActiveByUserNameOrEmailAsync(
            input.UserNameOrEmail,
            cancellationToken);

        if (user is null)
        {
            return Fail("Invalid username email or password.");
        }

        var verify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, input.Password);
        if (verify == PasswordVerificationResult.Failed)
        {
            return Fail("Invalid username email or password.");
        }

        if (user.IsPlatformSuperAdmin)
        {
            // Super Admin: email OTP required before session cookie.
            return await BeginPlatformOtpAsync(user, input.RememberMe, cancellationToken);
        }

        var store = await _authRepository.GetPreferredStoreAsync(user.IdUser, cancellationToken);
        if (store is null)
        {
            return Fail("No active store access is assigned to this user.");
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _authRepository.UpdateUserAsync(user, cancellationToken);

        return await IssueSessionAsync(user, store, input.RememberMe, isPlatformSuperAdmin: false, cancellationToken);
    }

    public async Task<OutputAuthSession> VerifyOtpAsync(
        InputAuthVerifyOtp input,
        CancellationToken cancellationToken = default)
    {
        var challenge = await _authRepository.GetOtpChallengeAsync(input.ChallengeToken.Trim(), cancellationToken);
        if (challenge is null || challenge.ConsumedAt is not null)
        {
            return Fail("This verification code is invalid or has already been used.");
        }

        if (challenge.ExpiresAt <= DateTime.UtcNow)
        {
            return Fail("This verification code has expired. Please sign in again.");
        }

        if (challenge.AttemptCount >= challenge.MaxAttempts)
        {
            return Fail("Too many incorrect attempts. Please sign in again.");
        }

        var user = await _authRepository.GetByIdAsync(challenge.FkUser, cancellationToken);
        if (user is null || !user.IsPlatformSuperAdmin)
        {
            return Fail("This verification code is invalid or has already been used.");
        }

        var otpCheck = _passwordHasher.VerifyHashedPassword(user, challenge.OtpHash, input.Code.Trim());
        if (otpCheck == PasswordVerificationResult.Failed)
        {
            challenge.AttemptCount += 1;
            await _authRepository.UpdateOtpChallengeAsync(challenge, cancellationToken);
            return Fail("Invalid verification code.");
        }

        challenge.ConsumedAt = DateTime.UtcNow;
        await _authRepository.UpdateOtpChallengeAsync(challenge, cancellationToken);

        user.LastLoginAt = DateTime.UtcNow;
        await _authRepository.UpdateUserAsync(user, cancellationToken);

        return await IssueSuperAdminSessionAsync(user, challenge.RememberMe, cancellationToken);
    }

    public async Task<OutputAuthSession> ResendOtpAsync(
        InputAuthResendOtp input,
        CancellationToken cancellationToken = default)
    {
        var existing = await _authRepository.GetOtpChallengeAsync(input.ChallengeToken.Trim(), cancellationToken);
        if (existing is null || existing.ConsumedAt is not null)
        {
            return Fail("This verification session is no longer valid. Please sign in again.");
        }

        var user = await _authRepository.GetByIdAsync(existing.FkUser, cancellationToken);
        if (user is null || !user.IsPlatformSuperAdmin)
        {
            return Fail("This verification session is no longer valid. Please sign in again.");
        }

        return await BeginPlatformOtpAsync(user, existing.RememberMe, cancellationToken);
    }

    public async Task<OutputAuthSession> ChangePasswordAsync(
        InputAuthChangePassword input,
        CancellationToken cancellationToken = default)
    {
        var user = await _authRepository.GetByIdAsync(input.IdUser, cancellationToken);
        if (user is null)
        {
            return Fail("User not found.");
        }

        var verify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, input.CurrentPassword);
        if (verify == PasswordVerificationResult.Failed)
        {
            return Fail("Current password is incorrect.");
        }

        user.PasswordHash = _passwordHasher.HashPassword(user, input.NewPassword);
        user.MustChangePassword = false;
        await _authRepository.UpdateUserAsync(user, cancellationToken);

        if (user.IsPlatformSuperAdmin)
        {
            return await IssueSuperAdminSessionAsync(user, rememberMe: false, cancellationToken);
        }

        var store = await _authRepository.GetPreferredStoreAsync(user.IdUser, cancellationToken);
        if (store is null)
        {
            return Fail("Password updated, but no store access was found. Contact an administrator.");
        }

        return await IssueSessionAsync(user, store, rememberMe: false, isPlatformSuperAdmin: false, cancellationToken);
    }

    public async Task<OutputAuthForgotPassword> RequestPasswordResetAsync(
        InputAuthForgotPassword input,
        CancellationToken cancellationToken = default)
    {
        var user = await _authRepository.FindActiveByUserNameOrEmailAsync(input.UserNameOrEmail, cancellationToken);
        if (user is null || string.IsNullOrWhiteSpace(user.Email))
        {
            // Same answer as success so the form cannot be used to discover which accounts exist.
            return new OutputAuthForgotPassword { Success = true, Message = ForgotPasswordGenericMessage };
        }

        var origin = ResolvePublicOrigin(input.RequestOrigin);
        var minutes = Math.Max(5, _otpSettings.PasswordResetExpiryMinutes);
        var token = CreateResetToken(user, TimeSpan.FromMinutes(minutes));
        var link = $"{origin}{input.ResetPath}?token={Uri.EscapeDataString(token)}";

        try
        {
            await _emailService.SendAsync(
                user.Email,
                "SmartCart Pro — reset your password",
                $"Hello {user.FullName},\n\n" +
                "We received a request to reset the password for your SmartCart Pro account.\n" +
                $"Open this link to choose a new password (valid for {minutes} minutes, one use):\n\n" +
                $"{link}\n\n" +
                "If you did not request this, you can ignore this email — your password stays the same.",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}", user.Email);
            throw new ServiceUnavailableException(
                "We could not send the reset email right now. Please try again later.",
                "SMTP send failed for password reset: " + ex.Message);
        }

        return new OutputAuthForgotPassword { Success = true, Message = ForgotPasswordGenericMessage };
    }

    public async Task<OutputAuthResetTokenSelect> GetPasswordResetTokenAsync(
        InputAuthResetTokenSelect input,
        CancellationToken cancellationToken = default)
    {
        var (user, _) = await ResolveResetTokenAsync(input.Token, cancellationToken);
        return user is null
            ? new OutputAuthResetTokenSelect { IsValid = false, ErrorMessage = ResetLinkInvalidMessage }
            : new OutputAuthResetTokenSelect { IsValid = true };
    }

    public async Task<OutputAuthResetPassword> ResetPasswordAsync(
        InputAuthResetPassword input,
        CancellationToken cancellationToken = default)
    {
        var (user, _) = await ResolveResetTokenAsync(input.Token, cancellationToken);
        if (user is null)
        {
            return new OutputAuthResetPassword { Success = false, ErrorMessage = ResetLinkInvalidMessage };
        }

        // New hash changes the token fingerprint, so this link (and any older one) cannot be reused.
        user.PasswordHash = _passwordHasher.HashPassword(user, input.NewPassword);
        user.MustChangePassword = false;
        user.SessionVersion += 1;
        user.UpdatedAt = DateTime.UtcNow;
        await _authRepository.UpdateUserAsync(user, cancellationToken);

        return new OutputAuthResetPassword { Success = true };
    }

    public async Task<OutputAuthSession> SwitchStoreAsync(
        int idUser,
        int fkStore,
        bool rememberMe,
        CancellationToken cancellationToken = default)
    {
        var user = await _authRepository.GetByIdAsync(idUser, cancellationToken);
        if (user is null)
        {
            return Fail("User not found.");
        }

        var store = user.IsPlatformSuperAdmin
            ? await _authRepository.GetActiveStoreByIdAsync(fkStore, cancellationToken)
            : await _authRepository.GetStoreAccessAsync(idUser, fkStore, cancellationToken);
        if (store is null)
        {
            return Fail(user.IsPlatformSuperAdmin
                ? "That store is not available."
                : "You do not have access to that store.");
        }

        return await IssueSessionAsync(user, store, rememberMe, isPlatformSuperAdmin: user.IsPlatformSuperAdmin, cancellationToken);
    }

    public void SetAuthCookie(string token, DateTime? expiresAt, bool rememberMe)
    {
        var response = _httpContextAccessor.HttpContext?.Response
            ?? throw new InvalidOperationException("No active HTTP context for auth cookie.");
        var isHttps = _httpContextAccessor.HttpContext.Request.IsHttps;
        var expires = expiresAt ?? DateTime.UtcNow.AddHours(8);

        response.Cookies.Append(AuthClaims.TokenCookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = isHttps,
            SameSite = SameSiteMode.Lax,
            IsEssential = true,
            Expires = rememberMe ? expires : null
        });
    }

    public void ClearAuthCookie()
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("No active HTTP context for auth cookie.");

        httpContext.Response.Cookies.Delete(AuthClaims.TokenCookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = httpContext.Request.IsHttps,
            SameSite = SameSiteMode.Lax
        });
    }

    private async Task<OutputAuthSession> BeginPlatformOtpAsync(
        User user,
        bool rememberMe,
        CancellationToken cancellationToken)
    {
        await _authRepository.InvalidateOpenOtpChallengesAsync(user.IdUser, cancellationToken);

        // Random code by default. A fixed code is honoured ONLY in the Development environment
        // and only when OtpSettings:DevelopmentFixedCode is set explicitly.
        var useDevelopmentCode = _hostEnvironment.IsDevelopment()
            && !string.IsNullOrWhiteSpace(_otpSettings.DevelopmentFixedCode);
        var otp = useDevelopmentCode
            ? _otpSettings.DevelopmentFixedCode!.Trim()
            : GenerateNumericOtp(_otpSettings.Length);

        var challengeToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
        var challenge = new AuthOtpChallenge
        {
            ChallengeToken = challengeToken,
            FkUser = user.IdUser,
            OtpHash = _passwordHasher.HashPassword(user, otp),
            RememberMe = rememberMe,
            AttemptCount = 0,
            MaxAttempts = Math.Max(1, _otpSettings.MaxAttempts),
            ExpiresAt = DateTime.UtcNow.AddMinutes(Math.Max(1, _otpSettings.ExpiryMinutes)),
            CreatedAt = DateTime.UtcNow
        };

        await _authRepository.AddOtpChallengeAsync(challenge, cancellationToken);

        try
        {
            await _emailService.SendAsync(
                user.Email,
                "SmartCart Pro — Super Admin verification code",
                $"Your SmartCart Pro Super Admin verification code is: {otp}\n\n" +
                $"This code expires in {_otpSettings.ExpiryMinutes} minutes.\n" +
                "If you did not request this, ignore this email.",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send Super Admin OTP email to {Email}", user.Email);
            if (!useDevelopmentCode)
            {
                return Fail("Could not send the verification email. Check SMTP settings and try again.");
            }

            _logger.LogWarning("Continuing because OtpSettings:DevelopmentFixedCode is active in Development.");
        }

        return new OutputAuthSession
        {
            Success = true,
            RequiresOtp = true,
            OtpChallengeToken = challengeToken,
            MaskedEmail = MaskEmail(user.Email),
            IsPlatformSuperAdmin = true,
            FullName = user.FullName
        };
    }

    private async Task<OutputAuthSession> IssueSuperAdminSessionAsync(
        User user,
        bool rememberMe,
        CancellationToken cancellationToken)
    {
        var store = await _authRepository.GetAnyActiveStoreAsync(cancellationToken);
        if (store is not null)
        {
            return await IssueSessionAsync(user, store, rememberMe, isPlatformSuperAdmin: true, cancellationToken);
        }

        var sessionVersion = await NextSessionVersionAsync(user, cancellationToken);
        var (token, expiresAt) = _jwtTokenService.CreateToken(
            user.IdUser,
            user.UserName,
            user.FullName,
            user.Email,
            fkStore: 0,
            fkCompany: 0,
            storeCode: string.Empty,
            storeName: string.Empty,
            mustChangePassword: user.MustChangePassword,
            rememberMe: rememberMe,
            isPlatformSuperAdmin: true,
            sessionVersion: sessionVersion);

        return new OutputAuthSession
        {
            Success = true,
            Token = token,
            ExpiresAt = expiresAt,
            MustChangePassword = user.MustChangePassword,
            IsPlatformSuperAdmin = true,
            Fk_Store = 0,
            Fk_Company = 0,
            StoreName = string.Empty,
            FullName = user.FullName
        };
    }

    private async Task<int> NextSessionVersionAsync(User user, CancellationToken cancellationToken)
    {
        user.SessionVersion += 1;
        user.UpdatedAt = DateTime.UtcNow;
        await _authRepository.UpdateUserAsync(user, cancellationToken);
        return user.SessionVersion;
    }

    private async Task<OutputAuthSession> IssueSessionAsync(
        User user,
        AuthUserStoreInfo store,
        bool rememberMe,
        bool isPlatformSuperAdmin,
        CancellationToken cancellationToken)
    {
        var sessionVersion = await NextSessionVersionAsync(user, cancellationToken);
        var (token, expiresAt) = _jwtTokenService.CreateToken(
            user.IdUser,
            user.UserName,
            user.FullName,
            user.Email,
            store.Fk_Store,
            store.Fk_Company,
            store.StoreCode,
            store.StoreName,
            user.MustChangePassword,
            rememberMe,
            isPlatformSuperAdmin,
            sessionVersion);

        return new OutputAuthSession
        {
            Success = true,
            Token = token,
            ExpiresAt = expiresAt,
            MustChangePassword = user.MustChangePassword,
            IsPlatformSuperAdmin = isPlatformSuperAdmin,
            Fk_Store = store.Fk_Store,
            Fk_Company = store.Fk_Company,
            StoreName = store.StoreName,
            FullName = user.FullName
        };
    }

    // ── Password reset token (stateless, time-limited, single use) ──────────────────────

    /// <summary>
    /// Token = time-limited data-protection payload "idUser|fingerprint". The fingerprint is derived from the
    /// current password hash, so the token stops working as soon as the password changes (single use).
    /// </summary>
    private string CreateResetToken(User user, TimeSpan lifetime)
    {
        var payload = $"{user.IdUser}|{PasswordFingerprint(user)}";
        return ResetProtector().Protect(payload, lifetime);
    }

    private async Task<(User? User, string? Error)> ResolveResetTokenAsync(
        string token,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return (null, ResetLinkInvalidMessage);
        }

        string payload;
        try
        {
            payload = ResetProtector().Unprotect(token.Trim());
        }
        catch (CryptographicException)
        {
            // Tampered, wrong key, or expired.
            return (null, ResetLinkInvalidMessage);
        }

        var parts = payload.Split('|');
        if (parts.Length != 2 || !int.TryParse(parts[0], out var idUser) || idUser <= 0)
        {
            return (null, ResetLinkInvalidMessage);
        }

        var user = await _authRepository.GetByIdAsync(idUser, cancellationToken);
        if (user is null
            || !CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(parts[1]),
                Encoding.UTF8.GetBytes(PasswordFingerprint(user))))
        {
            return (null, ResetLinkInvalidMessage);
        }

        return (user, null);
    }

    private ITimeLimitedDataProtector ResetProtector() =>
        _dataProtectionProvider.CreateProtector(ResetTokenPurpose).ToTimeLimitedDataProtector();

    private static string PasswordFingerprint(User user)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(user.PasswordHash ?? string.Empty));
        return Convert.ToHexString(hash, 0, 8);
    }

    /// <summary>
    /// Origin for emailed links. Production must set EnvironmentSettings:PublicBaseUrl so a forged Host header
    /// cannot redirect reset links to another site; Development may fall back to the current request origin.
    /// </summary>
    private string ResolvePublicOrigin(string requestOrigin)
    {
        var configured = _environmentSettings.PublicBaseUrl?.Trim().TrimEnd('/');
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return configured;
        }

        if (_hostEnvironment.IsDevelopment() && !string.IsNullOrWhiteSpace(requestOrigin))
        {
            return requestOrigin.TrimEnd('/');
        }

        throw new ServiceUnavailableException(
            "Password reset is not available right now. Please contact your administrator.",
            "EnvironmentSettings:PublicBaseUrl is not configured; refusing to build a reset link from the request Host header.");
    }

    private static OutputAuthSession Fail(string message) =>
        new() { Success = false, ErrorMessage = message };

    private static string GenerateNumericOtp(int length)
    {
        length = Math.Clamp(length, 4, 8);
        var max = (int)Math.Pow(10, length);
        var value = RandomNumberGenerator.GetInt32(0, max);
        return value.ToString($"D{length}");
    }

    private static string MaskEmail(string email)
    {
        var at = email.IndexOf('@');
        if (at <= 1)
        {
            return "***";
        }

        return email[0] + new string('*', Math.Min(at - 1, 6)) + email[at..];
    }
}
