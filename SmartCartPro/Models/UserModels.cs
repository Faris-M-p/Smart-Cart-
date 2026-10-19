using System.ComponentModel.DataAnnotations;
using SmartCartPro.CustomModelValidation;

namespace SmartCartPro.Models;

#region User Options (role and store pickers)

/// <summary>Criteria for the role and store pickers used by the employee list filters and forms.</summary>
public sealed class InputUserOptionList
{
    /// <summary>Company whose stores are offered. Null or 0 = every store (platform super admin).</summary>
    public int? FkCompany { get; set; }
}

public sealed class OutputUserOptionList
{
    public List<OutputUserRoleSelect> Roles { get; set; } = new();
    public List<OutputUserStoreSelect> Stores { get; set; } = new();
}

public sealed class OutputUserRoleSelect
{
    public int IdRole { get; set; }
    public string RoleCode { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public sealed class OutputUserStoreSelect
{
    public int IdStore { get; set; }
    public string StoreCode { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

#endregion

#region User List

public sealed class InputUserList
{
    /// <summary>Matches full name, username, email or employee code (contains, case-insensitive).</summary>
    public string? Search { get; set; }

    public int? FkRole { get; set; }

    public int? FkStore { get; set; }

    /// <summary>Null = all, true = active only, false = inactive only.</summary>
    public bool? ActiveOnly { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 25;

    /// <summary>fullname (default) | username | email | role | lastlogin | isactive.</summary>
    public string SortBy { get; set; } = "fullname";

    public bool SortDesc { get; set; }
}

public sealed class OutputUserList
{
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<OutputUserSelect> Users { get; set; } = new();
    public InputUserList Filter { get; set; } = new();
}

#endregion

#region User Select

public sealed class InputUserSelect
{
    public int IdUser { get; set; }
}

public sealed class OutputUserSelect
{
    public int IdUser { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? EmployeeCode { get; set; }
    public bool IsActive { get; set; }
    public bool MustChangePassword { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public int? CurrentRoleId { get; set; }
    public string? CurrentRoleName { get; set; }

    /// <summary>Active (not cancelled) store access rows, default store first.</summary>
    public List<OutputUserStoreAccess> Stores { get; set; } = new();
}

public sealed class OutputUserStoreAccess
{
    public int IdStore { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
}

#endregion

#region User Create

public sealed class InUserCreateView : IValidatableObject
{
    [Display(Name = "Username")]
    [Required(ErrorMessage = "The Username field is required.")]
    [StringLengthValidation(3, 50, ErrorMessage = "The Username must be between 3 and 50 characters.")]
    [MatchRegexAllowNull(@"^[A-Za-z0-9][A-Za-z0-9._-]*$", ErrorMessage = "The Username can contain letters, numbers, dots, dashes and underscores only.")]
    [NoScriptTags(ErrorMessage = "The Username cannot contain script tags.")]
    public string UserName { get; set; } = string.Empty;

    [Display(Name = "Email")]
    [Required(ErrorMessage = "The Email field is required.")]
    [StringLengthValidation(5, 200, ErrorMessage = "The Email must be between 5 and 200 characters.")]
    [EmailIfNotEmpty(ErrorMessage = "The Email format is invalid.")]
    [MatchRegexAllowNull(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "The Email format is invalid.")]
    [NoScriptTags(ErrorMessage = "The Email cannot contain script tags.")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Full Name")]
    [Required(ErrorMessage = "The Full Name field is required.")]
    [TrimmedString(2, 200, ErrorMessage = "The Full Name must be between 2 and 200 characters.")]
    [NoScriptTags(ErrorMessage = "The Full Name cannot contain script tags.")]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "Phone")]
    [StringLength(15, ErrorMessage = "The Phone cannot exceed 15 characters.")]
    [PhoneIfNotEmpty]
    [PhoneNumber(ErrorMessage = "Invalid phone number format.")]
    public string? Phone { get; set; }

    [Display(Name = "Employee Code")]
    [StringLengthValidation(0, 50, ErrorMessage = "The Employee Code cannot exceed 50 characters.")]
    [NoScriptTags(ErrorMessage = "The Employee Code cannot contain script tags.")]
    public string? EmployeeCode { get; set; }

    [Display(Name = "Role")]
    [Required(ErrorMessage = "Please select a role.")]
    [GreaterThanZero(ErrorMessage = "Please select a role.")]
    public int? FkRole { get; set; }

    [Display(Name = "Store Access")]
    [NotEmptyList(ErrorMessage = "Select at least one store. Employees need a store to sign in.")]
    public List<int> StoreIds { get; set; } = new();

    [Display(Name = "Default Store")]
    [GreaterThanOrEqualToZero(ErrorMessage = "Please choose a valid default store.")]
    public int? DefaultStoreId { get; set; }

    [Display(Name = "Generate a temporary password")]
    public bool GeneratePassword { get; set; } = true;

    [Display(Name = "Initial Password")]
    [StrongPassword(8, 64)]
    public string? InitialPassword { get; set; }

    [Display(Name = "Require a password change at next sign-in")]
    public bool MustChangePassword { get; set; } = true;

    /// <summary>The initial password is only required when the admin chooses to type one.</summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!GeneratePassword && string.IsNullOrWhiteSpace(InitialPassword))
        {
            yield return new ValidationResult(
                "The Initial Password field is required.",
                new[] { nameof(InitialPassword) });
        }
    }
}

public sealed class InputUserCreate
{
    public int ActorUserId { get; set; }

    /// <summary>Company of the signed-in actor (claim). Null or 0 for platform super admin.</summary>
    public int? ActorFkCompany { get; set; }

    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? EmployeeCode { get; set; }
    public int FkRole { get; set; }
    public List<int> StoreIds { get; set; } = new();
    public int? DefaultStoreId { get; set; }
    public bool GeneratePassword { get; set; }

    /// <summary>Admin-typed password (plain text, memory only). Cleared by the service after hashing.</summary>
    public string? InitialPassword { get; set; }

    public bool MustChangePassword { get; set; }

    /// <summary>Set by the service from the chosen or generated password. Never a plain password.</summary>
    public string PasswordHash { get; set; } = string.Empty;
}

public sealed class OutputUserCreate
{
    public int IdUser { get; set; }
    public string UserName { get; set; } = string.Empty;

    /// <summary>Generated temporary password. Returned once in the create response and never stored in plain text.</summary>
    public string? TemporaryPassword { get; set; }
}

#endregion

#region User Update

public sealed class InUserUpdateView
{
    [Display(Name = "User")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Something went wrong/Please select a valid User.")]
    public int IdUser { get; set; }

    [Display(Name = "Email")]
    [Required(ErrorMessage = "The Email field is required.")]
    [StringLengthValidation(5, 200, ErrorMessage = "The Email must be between 5 and 200 characters.")]
    [EmailIfNotEmpty(ErrorMessage = "The Email format is invalid.")]
    [MatchRegexAllowNull(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "The Email format is invalid.")]
    [NoScriptTags(ErrorMessage = "The Email cannot contain script tags.")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Full Name")]
    [Required(ErrorMessage = "The Full Name field is required.")]
    [TrimmedString(2, 200, ErrorMessage = "The Full Name must be between 2 and 200 characters.")]
    [NoScriptTags(ErrorMessage = "The Full Name cannot contain script tags.")]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "Phone")]
    [StringLength(15, ErrorMessage = "The Phone cannot exceed 15 characters.")]
    [PhoneIfNotEmpty]
    [PhoneNumber(ErrorMessage = "Invalid phone number format.")]
    public string? Phone { get; set; }

    [Display(Name = "Employee Code")]
    [StringLengthValidation(0, 50, ErrorMessage = "The Employee Code cannot exceed 50 characters.")]
    [NoScriptTags(ErrorMessage = "The Employee Code cannot contain script tags.")]
    public string? EmployeeCode { get; set; }

    [Display(Name = "Role")]
    [Required(ErrorMessage = "Please select a role.")]
    [GreaterThanZero(ErrorMessage = "Please select a role.")]
    public int? FkRole { get; set; }

    [Display(Name = "Store Access")]
    [NotEmptyList(ErrorMessage = "Select at least one store. Employees need a store to sign in.")]
    public List<int> StoreIds { get; set; } = new();

    [Display(Name = "Default Store")]
    [GreaterThanOrEqualToZero(ErrorMessage = "Please choose a valid default store.")]
    public int? DefaultStoreId { get; set; }

    // Read-only values shown on the edit page. They are not posted and never mapped to the input.
    [Display(Name = "Username")]
    public string? UserName { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Password change required")]
    public bool MustChangePassword { get; set; }

    [Display(Name = "Last sign-in")]
    public DateTime? LastLoginAt { get; set; }
}

public sealed class InputUserUpdate
{
    public int ActorUserId { get; set; }
    public int? ActorFkCompany { get; set; }
    public int IdUser { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? EmployeeCode { get; set; }
    public int FkRole { get; set; }
    public List<int> StoreIds { get; set; } = new();
    public int? DefaultStoreId { get; set; }
}

public sealed class OutputUserUpdate
{
    public int IdUser { get; set; }
}

#endregion

#region User Status

public sealed class InputUserStatusUpdate
{
    public int ActorUserId { get; set; }
    public int IdUser { get; set; }
    public bool IsActive { get; set; }
}

public sealed class OutputUserStatusUpdate
{
    public int IdUser { get; set; }
    public bool IsActive { get; set; }
}

#endregion

#region User Password Reset

public sealed class InUserPasswordUpdateView : IValidatableObject
{
    [Display(Name = "User")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Something went wrong/Please select a valid User.")]
    public int IdUser { get; set; }

    [Display(Name = "Generate a temporary password")]
    public bool GeneratePassword { get; set; } = true;

    [Display(Name = "New Password")]
    [StrongPassword(8, 64)]
    public string? NewPassword { get; set; }

    /// <summary>The new password is only required when the admin chooses to type one.</summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!GeneratePassword && string.IsNullOrWhiteSpace(NewPassword))
        {
            yield return new ValidationResult(
                "The New Password field is required.",
                new[] { nameof(NewPassword) });
        }
    }
}

public sealed class InputUserPasswordUpdate
{
    public int ActorUserId { get; set; }
    public int IdUser { get; set; }
    public bool GeneratePassword { get; set; }

    /// <summary>Admin-typed password (plain text, memory only). Cleared by the service after hashing.</summary>
    public string? NewPassword { get; set; }

    /// <summary>Set by the service from the chosen or generated password. Never a plain password.</summary>
    public string PasswordHash { get; set; } = string.Empty;
}

public sealed class OutputUserPasswordUpdate
{
    public int IdUser { get; set; }

    /// <summary>Generated temporary password. Returned once in the response and never stored in plain text.</summary>
    public string? TemporaryPassword { get; set; }
}

#endregion
