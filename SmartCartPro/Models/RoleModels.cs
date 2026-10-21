using System.ComponentModel.DataAnnotations;
using SmartCartPro.CustomModelValidation;

namespace SmartCartPro.Models;

#region Permission Select

/// <summary>One row of the seeded permission catalog, with its module for grouping in the matrix.</summary>
public sealed class OutputPermissionSelect
{
    public int IdPermission { get; set; }
    public string PermissionCode { get; set; } = string.Empty;
    public string PermissionName { get; set; } = string.Empty;
    public int FkModule { get; set; }
    public string ModuleCode { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public int ModuleDisplayOrder { get; set; }
    public int DisplayOrder { get; set; }
}

#endregion

#region Role List

public sealed class InputRoleList
{
    /// <summary>Matches role code, name or description (contains, case-insensitive).</summary>
    public string? Search { get; set; }

    /// <summary>Null = all, true = active only, false = inactive only.</summary>
    public bool? ActiveOnly { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 25;

    /// <summary>rolename (default) | rolecode | usercount | isactive.</summary>
    public string SortBy { get; set; } = "rolename";

    public bool SortDesc { get; set; }
}

public sealed class OutputRoleList
{
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<OutputRoleSelect> Roles { get; set; } = new();
    public InputRoleList Filter { get; set; } = new();
}

#endregion

#region Role Select

public sealed class InputRoleSelect
{
    public int IdRole { get; set; }

    /// <summary>True when the signed-in actor is the platform super admin. The ADMIN role stays read-only for this actor too.</summary>
    public bool ActorIsPlatformAdmin { get; set; }
}

public sealed class OutputRoleSelect
{
    public int IdRole { get; set; }
    public string RoleCode { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; }

    /// <summary>Number of employees (not cancelled) that hold this role.</summary>
    public int UserCount { get; set; }

    public int PermissionCount { get; set; }
    public List<int> PermissionIds { get; set; } = new();

    /// <summary>True for the built-in Admin role: name, code and permissions are read-only for every actor.</summary>
    public bool IsLocked { get; set; }
}

#endregion

#region Role Create

public sealed class InRoleCreateView
{
    [Display(Name = "Role Code")]
    [Required(ErrorMessage = "The Role Code field is required.")]
    [StringLengthValidation(2, 30, ErrorMessage = "The Role Code must be between 2 and 30 characters.")]
    [MatchRegexAllowNull(@"^[A-Za-z][A-Za-z0-9_]*$", ErrorMessage = "The Role Code must start with a letter and use letters, numbers and underscores only.")]
    [NoScriptTags(ErrorMessage = "The Role Code cannot contain script tags.")]
    public string RoleCode { get; set; } = string.Empty;

    [Display(Name = "Role Name")]
    [Required(ErrorMessage = "The Role Name field is required.")]
    [TrimmedString(2, 100, ErrorMessage = "The Role Name must be between 2 and 100 characters.")]
    [NoScriptTags(ErrorMessage = "The Role Name cannot contain script tags.")]
    public string RoleName { get; set; } = string.Empty;

    [Display(Name = "Description")]
    [StringLengthValidation(0, 500, ErrorMessage = "The Description cannot exceed 500 characters.")]
    [NoScriptTags(ErrorMessage = "The Description cannot contain script tags.")]
    public string? Description { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Permissions")]
    public List<int> PermissionIds { get; set; } = new();
}

public sealed class InputRoleCreate
{
    public int ActorUserId { get; set; }
    public string RoleCode { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public List<int> PermissionIds { get; set; } = new();
}

public sealed class OutputRoleCreate
{
    public int IdRole { get; set; }
    public string RoleCode { get; set; } = string.Empty;
}

#endregion

#region Role Update

public sealed class InRoleUpdateView
{
    [Display(Name = "Role")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Something went wrong/Please select a valid Role.")]
    public int IdRole { get; set; }

    [Display(Name = "Role Name")]
    [Required(ErrorMessage = "The Role Name field is required.")]
    [TrimmedString(2, 100, ErrorMessage = "The Role Name must be between 2 and 100 characters.")]
    [NoScriptTags(ErrorMessage = "The Role Name cannot contain script tags.")]
    public string RoleName { get; set; } = string.Empty;

    [Display(Name = "Description")]
    [StringLengthValidation(0, 500, ErrorMessage = "The Description cannot exceed 500 characters.")]
    [NoScriptTags(ErrorMessage = "The Description cannot contain script tags.")]
    public string? Description { get; set; }

    [Display(Name = "Permissions")]
    public List<int> PermissionIds { get; set; } = new();

    // Read-only values shown on the edit page. They are not posted and never mapped to the input.
    [Display(Name = "Role Code")]
    public string? RoleCode { get; set; }

    [Display(Name = "System Role")]
    public bool IsSystem { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Read-only")]
    public bool IsLocked { get; set; }

    [Display(Name = "Employees")]
    public int UserCount { get; set; }
}

public sealed class InputRoleUpdate
{
    public int ActorUserId { get; set; }
    public bool ActorIsPlatformAdmin { get; set; }
    public int IdRole { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<int> PermissionIds { get; set; } = new();
}

public sealed class OutputRoleUpdate
{
    public int IdRole { get; set; }
}

#endregion

#region Role Status

public sealed class InputRoleStatusUpdate
{
    public int ActorUserId { get; set; }
    public int IdRole { get; set; }
    public bool IsActive { get; set; }
}

public sealed class OutputRoleStatusUpdate
{
    public int IdRole { get; set; }
    public bool IsActive { get; set; }
}

#endregion

#region Role Delete

public sealed class InputRoleDelete
{
    public int ActorUserId { get; set; }
    public int IdRole { get; set; }
}

public sealed class OutputRoleDelete
{
    public int IdRole { get; set; }
}

#endregion
