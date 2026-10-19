using System.ComponentModel.DataAnnotations;
using SmartCartPro.CustomModelValidation;

namespace SmartCartPro.Models;

#region User List

public sealed class InputUserList
{
    public bool ActiveOnly { get; set; } = false;
}

public sealed class OutputUserList
{
    public List<OutputUserSelect> Users { get; set; } = new();
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
    public List<int> StoreIds { get; set; } = new();
}

#endregion

#region User Create

public sealed class InUserCreateView
{
    [Display(Name = "Username")]
    [Required(ErrorMessage = "The Username field is required.")]
    [StringLengthValidation(3, 50, ErrorMessage = "The Username must be between 3 and 50 characters.")]
    [NoScriptTags(ErrorMessage = "The Username cannot contain script tags.")]
    public string UserName { get; set; } = string.Empty;

    [Display(Name = "Email")]
    [Required(ErrorMessage = "The Email field is required.")]
    [EmailIfNotEmpty]
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
    [GreaterThanOrEqualToZero(ErrorMessage = "Something went wrong/Please select a valid Role.")]
    public int? FkRole { get; set; }

    [Display(Name = "Store Access")]
    public List<int> StoreIds { get; set; } = new();

    [Display(Name = "Initial Password")]
    [Required(ErrorMessage = "The Initial Password field is required.")]
    [StrongPassword(8, 64, ErrorMessage = "The Initial Password must be between 8 and 64 characters and contain uppercase, lowercase, digit, and special character.")]
    public string InitialPassword { get; set; } = string.Empty;
}

public sealed class InputUserCreate
{
    public int ActorUserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? EmployeeCode { get; set; }
    public int? FkRole { get; set; }
    public List<int> StoreIds { get; set; } = new();
    public string PasswordHash { get; set; } = string.Empty;
}

public sealed class OutputUserCreate
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public int IdUser { get; set; }
    public string UserName { get; set; } = string.Empty;
    public bool Ok => ResponseCode == 0;
}

#endregion

#region User Update

public sealed class InUserUpdateView
{
    [Display(Name = "User")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Something went wrong/Please select a valid User.")]
    public int IdUser { get; set; }

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

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}

public sealed class InputUserUpdate
{
    public int ActorUserId { get; set; }
    public int IdUser { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? EmployeeCode { get; set; }
    public bool IsActive { get; set; }
}

public sealed class OutputUserUpdate
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public bool Ok => ResponseCode == 0;
}

#endregion
