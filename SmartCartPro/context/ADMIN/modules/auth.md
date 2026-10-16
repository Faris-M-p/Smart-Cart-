# Admin Module — Auth

## Module Overview
- **Area:** ADMIN
- **Module Name:** Auth
- **Current Status:** `IMPLEMENTED`

## Purpose & Responsibilities
Shared employee + platform Super Admin sign-in on `/Auth/Login`. Store admins get JWT after password. Platform Super Admin (`is_platform_super_admin`) gets email OTP, then JWT with platform claim. No public register.

## Implemented Features
- **Login (shared):** password verify; store admin → cookie; Super Admin → OTP email → `/Auth/VerifyOtp`
- **OTP:** random 6-digit, hashed in `auth_otp_challenges`, 5 min expiry, attempt limit, SMTP send (`SmtpSettings`)
- **Resend OTP:** invalidates prior open challenges, sends new code
- **Change password / Logout**
- **Seed:** `02_Seed/004_PlatformSuperAdmin.sql` — `Muhdfarismp@gmail.com` / `Admin@123` (dev)

## Data
- `users.is_platform_super_admin`
- `auth_otp_challenges`
- Models: `LoginInput`, `VerifyOtpInput`, `ChangePasswordInput`, `AuthSessionResult`
- Claims: `is_platform_super_admin`, `fk_store`, …

## Config
- `JwtSettings`, `SmtpSettings` (Host/Port/UserName/Password/FromEmail), `OtpSettings`

## Business Rules
- Super Admin login identifier is email (seed username = email)
- Store admin still needs `user_store_access`
- Super Admin session has no store (`fk_store=0`, badge “Super Admin”)
- Do not hardcode OTP; SMTP must be configured to complete Super Admin login

## Dependencies
- `super-admin.md`
