# Admin Module — Auth

## Module Overview
- **Area:** ADMIN
- **Module Name:** Auth
- **Current Status:** `IMPLEMENTED` (server + AJAX UI)

## Purpose & Responsibilities
Shared employee + platform Super Admin sign-in on `/Auth/Login`. Store admins get a JWT cookie after password. Platform Super Admin (`is_platform_super_admin`) gets an email OTP, then a JWT with the platform claim. No public register. Password recovery by emailed link.

## Implemented Features
- **Login (shared):** password verify; store user → cookie; Super Admin → OTP email → `/Auth/VerifyOtp`.
- **OTP:** random numeric code (`OtpSettings:Length`), hashed in `auth_otp_challenges`, expiry + attempt limit, SMTP send (`SmtpSettings`). **No hard-coded OTP.** For local development only, `OtpSettings:DevelopmentFixedCode` (set in the untracked `appsettings.Development.json`) is honoured when the host environment is `Development`; it is ignored everywhere else.
- **Resend OTP:** invalidates prior open challenges, sends a new code, returns the new challenge token (JSON).
- **Change password / Logout** (`LogoutGet` link in the header, `Logout` POST).
- **Forgot / Reset password:** `ForgotPassword` POST emails a link `…/Auth/ResetPassword?token=…`; the response is identical whether or not the account exists. Token = ASP.NET Data Protection time-limited payload (`OtpSettings:PasswordResetExpiryMinutes`, default 30) bound to a fingerprint of the current password hash, so it is **single use** (changing the password invalidates it) and needs **no table**. Link host comes from `EnvironmentSettings:PublicBaseUrl` (required outside Development; Development falls back to the request origin) to prevent Host-header poisoning. In production persist the Data Protection key ring so links survive restarts.
- **Seed:** `02_Seed/004_PlatformSuperAdmin.sql` — `Muhdfarismp@gmail.com` / `Admin@123` (dev; force a change on first use).

## UI (AJAX, common JSON)
- Views in `Views/Auth/` (`Login`, `VerifyOtp`, `ChangePassword`, `ForgotPassword`, `ResetPassword`) on `_AuthLayout`; one script `wwwroot/js/auth/auth.js` using `SmartCartAjax.submitForm`.
- Every POST returns `CommonResponseModel`: success → `data.redirectUrl` (login → OTP page, home, change-password, or safe local `returnUrl`); failure → `ValidationException` message + `validationErrors` under the fields. Bad credentials are a 400 (not 401) so the login page shows the message instead of treating it as an expired session.

## Data
- `users.is_platform_super_admin`, `users.must_change_password`
- `auth_otp_challenges`
- Models (`Models/AuthModels.cs`): `InLoginView`/`InputAuthLogin`, `InVerifyOtpView`/`InputAuthVerifyOtp`, `InputAuthResendOtp`, `InChangePasswordView`/`InputAuthChangePassword`, `InForgotPasswordView`/`InputAuthForgotPassword`/`OutputAuthForgotPassword`, `InResetPasswordView`/`InputAuthResetTokenSelect`/`OutputAuthResetTokenSelect`/`InputAuthResetPassword`/`OutputAuthResetPassword`, shared `OutputAuthSession`.
- Claims: `id_user`, `fk_store`, `fk_company`, `store_code`, `store_name`, `must_change_password`, `is_platform_super_admin`.

## Config
- `JwtSettings`, `SmtpSettings` (Host/Port/UserName/Password/FromEmail), `OtpSettings` (ExpiryMinutes, MaxAttempts, Length, PasswordResetExpiryMinutes, DevelopmentFixedCode), `EnvironmentSettings:PublicBaseUrl`.

## Business Rules
- Super Admin login identifier is email (seed username = email).
- Store users still need `user_store_access` (active store) to sign in.
- Super Admin session has no store (`fk_store=0`, badge “Super Admin”).
- New passwords: 8–64 chars with upper, lower, digit and special character (`[StrongPassword(8, 64)]`).
- SMTP must be configured for Super Admin OTP and password reset emails.

## Known gaps
- No failed-login lockout / rate limiting yet (OTP attempts are limited; password attempts are not).
- Auth repository still returns the `User` entity to `AuthService` (pre-dates the entity-isolation rule).

## Dependencies
- `super-admin.md`
