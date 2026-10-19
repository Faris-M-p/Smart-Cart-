using System.ComponentModel.DataAnnotations;
using SmartCartPro.CustomModelValidation;

namespace SmartCartPro.Models;

#region Permission Select

public sealed class OutputPermissionSelect
{
    public int IdPermission { get; set; }
    public string PermissionCode { get; set; } = string.Empty;
    public string PermissionName { get; set; } = string.Empty;
    public int FkModule { get; set; }
    public string ModuleCode { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

#endregion

#region Role List

public sealed class InputRoleList { }

public sealed class OutputRoleList
{
    public List<OutputRoleSelect> Roles { get; set; } = new();
}

#endregion

#region Role Select

public sealed class InputRoleSelect
{
    public int IdRole { get; set; }
}

public sealed class OutputRoleSelect
{
    public int IdRole { get; set; }
    public string RoleCode { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; }
    public int UserCount { get; set; }
    public List<int> PermissionIds { get; set; } = new();
}

#endregion

#region Role Create

public sealed class InRoleCreateView
{
    [Display(Name = "Role Code")]
    [Required(ErrorMessage = "The Role Code field is required.")]
    [StringLengthValidation(2, 30, ErrorMessage = "The Role Code must be between 2 and 30 characters.")]
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
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public int IdRole { get; set; }
    public bool Ok => ResponseCode == 0;
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

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Permissions")]
    public List<int> PermissionIds { get; set; } = new();

    [Display(Name = "System Role")]
    public bool IsSystem { get; set; }

    [Display(Name = "Role Code")]
    public string RoleCode { get; set; } = string.Empty;

    [Display(Name = "All Permissions")]
    public List<OutputPermissionSelect> AllPermissions { get; set; } = new();
}

public sealed class InputRoleUpdate
{
    public int ActorUserId { get; set; }
    public int IdRole { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public List<int> PermissionIds { get; set; } = new();
}

public sealed class OutputRoleUpdate
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public bool Ok => ResponseCode == 0;
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
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public bool Ok => ResponseCode == 0;
}

#endregion
