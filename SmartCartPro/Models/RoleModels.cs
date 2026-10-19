using System.ComponentModel.DataAnnotations;

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
    [Required]
    [MaxLength(30)]
    [Display(Name = "Role code")]
    public string RoleCode { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Role name")]
    public string RoleName { get; set; } = string.Empty;

    [Display(Name = "Description")]
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

    public static InputRoleCreate From(InRoleCreateView view, int actorUserId) => new()
    {
        ActorUserId = actorUserId,
        RoleCode = view.RoleCode,
        RoleName = view.RoleName,
        Description = view.Description,
        IsActive = view.IsActive,
        PermissionIds = view.PermissionIds ?? new List<int>()
    };
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
    public int IdRole { get; set; }

    [Required]
    [Display(Name = "Role name")]
    public string RoleName { get; set; } = string.Empty;

    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Permissions")]
    public List<int> PermissionIds { get; set; } = new();

    // Read-only display data
    public bool IsSystem { get; set; }
    public string RoleCode { get; set; } = string.Empty;
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

    public static InputRoleUpdate From(InRoleUpdateView view, int actorUserId) => new()
    {
        ActorUserId = actorUserId,
        IdRole = view.IdRole,
        RoleName = view.RoleName,
        Description = view.Description,
        IsActive = view.IsActive,
        PermissionIds = view.PermissionIds ?? new List<int>()
    };
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
