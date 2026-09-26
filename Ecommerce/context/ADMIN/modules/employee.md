# Admin Module — Employee (Admin Users)

## Module Overview
- **Area:** ADMIN (Back Office)
- **Module Name:** Employee (Admin Users)
- **Current Status:** `IMPLEMENTED`

## Purpose & Responsibilities
Manages administrative employees, employee user accounts, role assignments, profile information, and profile image uploads.

## Implemented Features
- **Employee Listing:** Paginated list of admin employees with search and role filter.
- **Create Employee:** Creates new `AdminUsers` account (UserName, Password hashed with Identity `PasswordHasher`, FullName, Email, Phone, `FK_UserRole`, `IsActive`, ProfileImage).
- **Update Employee:** Edit employee profile details, update role, toggle active status.
- **Password Reset:** Admin password reset for employee account.
- **Soft Delete:** Sets `Cancelled = 1`.

## Filters / Search / Sorting
- **Filters:** `UserRoleId`, `IsActive`.
- **Search:** `SearchText` matching UserName, FullName, Email, or Phone.

## Data
- Entities / DTOs: `AdminUser`, `EmployeeViewModel`, `EmployeeInputModel`.

## Database Dependencies
- Tables: `AdminUsers` (PK `ID_AdminUser`, FK `FK_UserRole`), `UserRoles`.
- Access: EF Core (`EcommerceDbContext`) & `EmployeeRepository`.

## API & Data Access
- Controller: `Controllers/Admin/EmployeeController.cs` (`[Route("Admin/Employee")]`).
- Interface & Repository: `IEmployeeInterface.cs` / `EmployeeRepository.cs`.
- Helper: `EmployeeHelper.cs`.
- Permissions: `Employee.View`, `Employee.Create`, `Employee.Update`, `Employee.Delete`.

## UI Rules & Conventions
- Razor view: `views/Admin/Employee/Index.cshtml`.
- Profile image uploaded via `CommonImageService` into `wwwroot/uploads/employees/`.

## Business Rules
- UserName must be unique across `AdminUsers`.
- Deactivating (`IsActive = 0`) or cancelling (`Cancelled = 1`) an employee account immediately revokes admin access.
- Passwords hashed using ASP.NET Identity `PasswordHasher<AdminUser>`.

## Dependencies & Related Modules
- `ADMIN/modules/auth.md` — Authenticates employee accounts.
- `ADMIN/modules/user-role.md` — Assigns permission roles to employees.
