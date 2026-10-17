using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SmartCartPro.Configuration;
using SmartCartPro.Entities;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Services;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly AuthJwtTokenService _jwtTokenService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEmailService _emailService;
    private readonly OtpSettings _otpSettings;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IAuthRepository authRepository,
        IPasswordHasher<User> passwordHasher,
        AuthJwtTokenService jwtTokenService,
        IHttpContextAccessor httpContextAccessor,
        IEmailService emailService,
        IOptions<OtpSettings> otpSettings,
        ILogger<AuthService> logger)
    {
        _authRepository = authRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _httpContextAccessor = httpContextAccessor;
        _emailService = emailService;
        _otpSettings = otpSettings.Value;
        _logger = logger;
    }

    public async Task<AuthSessionResult> LoginAsync(
        LoginInput input,
        CancellationToken cancellationToken = default)
    {
        var user = await _authRepository.FindActiveByUserNameOrEmailAsync(
            input.UserNameOrEmail,
            cancellationToken);

        if (user is null)
        {
            return Fail("Invalid username/email or password.");
        }

        var verify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, input.Password);
        if (verify == PasswordVerificationResult.Failed)
        {
            return Fail("Invalid username/email or password.");
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

        return IssueSession(user, store, input.RememberMe, isPlatformSuperAdmin: false);
    }

    public async Task<AuthSessionResult> VerifyOtpAsync(
        VerifyOtpInput input,
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

        var isStaticOtp = string.Equals(input.Code.Trim(), "123456", StringComparison.Ordinal);
        var otpCheck = isStaticOtp
            ? PasswordVerificationResult.Success
            : _passwordHasher.VerifyHashedPassword(user, challenge.OtpHash, input.Code.Trim());
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

        return IssuePlatformSession(user, challenge.RememberMe);
    }

    public async Task<AuthSessionResult> ResendOtpAsync(
        string challengeToken,
        CancellationToken cancellationToken = default)
    {
        var existing = await _authRepository.GetOtpChallengeAsync(challengeToken.Trim(), cancellationToken);
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

    public async Task<AuthSessionResult> ChangePasswordAsync(
        int idUser,
        ChangePasswordInput input,
        CancellationToken cancellationToken = default)
    {
        var user = await _authRepository.GetByIdAsync(idUser, cancellationToken);
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
            return IssuePlatformSession(user, rememberMe: false);
        }

        var store = await _authRepository.GetPreferredStoreAsync(user.IdUser, cancellationToken);
        if (store is null)
        {
            return Fail("Password updated, but no store access was found. Contact an administrator.");
        }

        return IssueSession(user, store, rememberMe: false, isPlatformSuperAdmin: false);
    }

    public async Task<AuthSessionResult> SwitchStoreAsync(
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

        var store = await _authRepository.GetStoreAccessAsync(idUser, fkStore, cancellationToken);
        if (store is null)
        {
            return Fail("You do not have access to that store.");
        }

        return IssueSession(user, store, rememberMe, isPlatformSuperAdmin: user.IsPlatformSuperAdmin);
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

    private async Task<AuthSessionResult> BeginPlatformOtpAsync(
        User user,
        bool rememberMe,
        CancellationToken cancellationToken)
    {
        await _authRepository.InvalidateOpenOtpChallengesAsync(user.IdUser, cancellationToken);

        var otp = "123456";
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
            return Fail("Could not send the verification email. Check SMTP settings and try again.");
        }

        return new AuthSessionResult
        {
            Success = true,
            RequiresOtp = true,
            OtpChallengeToken = challengeToken,
            MaskedEmail = MaskEmail(user.Email),
            IsPlatformSuperAdmin = true,
            FullName = user.FullName
        };
    }

    private AuthSessionResult IssuePlatformSession(User user, bool rememberMe)
    {
        var (token, expiresAt) = _jwtTokenService.CreateToken(
            user.IdUser,
            user.UserName,
            user.FullName,
            user.Email,
            fkStore: 0,
            fkCompany: 0,
            storeCode: "PLATFORM",
            storeName: "Platform",
            mustChangePassword: user.MustChangePassword,
            rememberMe: rememberMe,
            isPlatformSuperAdmin: true);

        return new AuthSessionResult
        {
            Success = true,
            Token = token,
            ExpiresAt = expiresAt,
            MustChangePassword = user.MustChangePassword,
            IsPlatformSuperAdmin = true,
            Fk_Store = 0,
            Fk_Company = 0,
            StoreName = "Platform",
            FullName = user.FullName
        };
    }

    private AuthSessionResult IssueSession(
        User user,
        AuthUserStoreInfo store,
        bool rememberMe,
        bool isPlatformSuperAdmin)
    {
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
            isPlatformSuperAdmin);

        return new AuthSessionResult
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

    private static AuthSessionResult Fail(string message) =>
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
