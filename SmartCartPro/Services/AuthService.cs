using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
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

    public AuthService(
        IAuthRepository authRepository,
        IPasswordHasher<User> passwordHasher,
        AuthJwtTokenService jwtTokenService,
        IHttpContextAccessor httpContextAccessor)
    {
        _authRepository = authRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _httpContextAccessor = httpContextAccessor;
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

        var store = await _authRepository.GetPreferredStoreAsync(user.IdUser, cancellationToken);
        if (store is null)
        {
            return Fail("No active store access is assigned to this user.");
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _authRepository.UpdateUserAsync(user, cancellationToken);

        return IssueSession(user, store, input.RememberMe);
    }

    public async Task<AuthSessionResult> RegisterAsync(
        RegisterInput input,
        CancellationToken cancellationToken = default)
    {
        if (await _authRepository.AnyCompanyExistsAsync(cancellationToken))
        {
            return Fail("Company already registered. Sign in, or ask an administrator to create your account.");
        }

        if (await _authRepository.IsUserNameOrEmailTakenAsync(input.UserName, input.Email, cancellationToken))
        {
            return Fail("Username or email is already in use.");
        }

        var companyCode = BuildCompanyCode(input.CompanyName);
        var storeName = string.IsNullOrWhiteSpace(input.StoreName)
            ? "Main Branch"
            : input.StoreName.Trim();

        var company = new Company
        {
            CompanyCode = companyCode,
            CompanyName = input.CompanyName.Trim(),
            Email = input.Email.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var store = new Store
        {
            StoreCode = "MAIN",
            StoreName = storeName,
            Email = input.Email.Trim(),
            IsDefault = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var user = new User
        {
            UserName = input.UserName.Trim(),
            Email = input.Email.Trim(),
            FullName = input.FullName.Trim(),
            IsActive = true,
            MustChangePassword = false,
            CreatedAt = DateTime.UtcNow
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, input.Password);

        var access = new UserStoreAccess
        {
            IsDefault = true,
            CreatedAt = DateTime.UtcNow
        };

        UserRole? userRole = null;
        var roleId = await _authRepository.GetRoleIdByCodeAsync("SUPER_ADMIN", cancellationToken);
        if (roleId is not null)
        {
            userRole = new UserRole
            {
                FkRole = roleId.Value,
                CreatedAt = DateTime.UtcNow
            };
        }

        await _authRepository.CreateBootstrapAdminAsync(
            company,
            store,
            user,
            access,
            userRole,
            cancellationToken);

        var storeInfo = new AuthUserStoreInfo
        {
            Fk_Store = store.IdStore,
            Fk_Company = company.IdCompany,
            StoreCode = store.StoreCode,
            StoreName = store.StoreName,
            IsDefault = true
        };

        return IssueSession(user, storeInfo, rememberMe: true);
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

        var store = await _authRepository.GetPreferredStoreAsync(user.IdUser, cancellationToken);
        if (store is null)
        {
            return Fail("Password updated, but no store access was found. Contact an administrator.");
        }

        return IssueSession(user, store, rememberMe: false);
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

        return IssueSession(user, store, rememberMe);
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

    private AuthSessionResult IssueSession(User user, AuthUserStoreInfo store, bool rememberMe)
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
            rememberMe);

        return new AuthSessionResult
        {
            Success = true,
            Token = token,
            ExpiresAt = expiresAt,
            MustChangePassword = user.MustChangePassword,
            Fk_Store = store.Fk_Store,
            Fk_Company = store.Fk_Company,
            StoreName = store.StoreName,
            FullName = user.FullName
        };
    }

    private static AuthSessionResult Fail(string message) =>
        new() { Success = false, ErrorMessage = message };

    private static string BuildCompanyCode(string companyName)
    {
        var slug = Regex.Replace(companyName.Trim().ToUpperInvariant(), @"[^A-Z0-9]+", "");
        if (string.IsNullOrWhiteSpace(slug))
        {
            slug = "COMPANY";
        }

        return slug.Length <= 12 ? slug : slug[..12];
    }
}
