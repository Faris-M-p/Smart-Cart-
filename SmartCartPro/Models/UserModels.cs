using System.ComponentModel.DataAnnotations;

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
    [Required]
    [MaxLength(50)]
    [Display(Name = "Username")]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "Phone")]
    public string? Phone { get; set; }

    [Display(Name = "Employee code")]
    public string? EmployeeCode { get; set; }

    [Display(Name = "Role")]
    public int? FkRole { get; set; }

    [Display(Name = "Store access")]
    public List<int> StoreIds { get; set; } = new();

    [Required]
    [MinLength(8)]
    [Display(Name = "Initial password")]
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
    public int IdUser { get; set; }

    [Required]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "Phone")]
    public string? Phone { get; set; }

    [Display(Name = "Employee code")]
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

    public static InputUserUpdate From(InUserUpdateView view, int actorUserId) => new()
    {
        ActorUserId = actorUserId,
        IdUser = view.IdUser,
        FullName = view.FullName,
        Phone = view.Phone,
        EmployeeCode = view.EmployeeCode,
        IsActive = view.IsActive
    };
}

public sealed class OutputUserUpdate
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public bool Ok => ResponseCode == 0;
}

#endregion
