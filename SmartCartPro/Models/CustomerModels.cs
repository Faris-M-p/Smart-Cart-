using System.ComponentModel.DataAnnotations;

namespace SmartCartPro.Models;

#region Customer Create

public sealed class InCustomerCreateView
{
    [MaxLength(20)]
    [Display(Name = "Customer code")]
    public string? CustomerCode { get; set; }

    [Required, MaxLength(300)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(200), EmailAddress]
    public string? Email { get; set; }

    [MaxLength(30)]
    public string? Phone { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}

public sealed class InputCustomerCreate
{
    public string? CustomerCode { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;

    public static InputCustomerCreate From(InCustomerCreateView v) => new()
    {
        CustomerCode = v.CustomerCode,
        FullName = v.FullName,
        Email = v.Email,
        Phone = v.Phone,
        IsActive = v.IsActive
    };
}

public sealed class OutputCustomerCreate
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public int IdCustomer { get; set; }
    public string FullName { get; set; } = string.Empty;
    public bool Ok => ResponseCode == 0;
}

#endregion

#region Customer Update

public sealed class InCustomerUpdateView
{
    public int IdCustomer { get; set; }

    [MaxLength(20)]
    [Display(Name = "Customer code")]
    public string? CustomerCode { get; set; }

    [Required, MaxLength(300)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(200), EmailAddress]
    public string? Email { get; set; }

    [MaxLength(30)]
    public string? Phone { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}

public sealed class InputCustomerUpdate
{
    public int IdCustomer { get; set; }
    public string? CustomerCode { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;

    public static InputCustomerUpdate From(InCustomerUpdateView v) => new()
    {
        IdCustomer = v.IdCustomer,
        CustomerCode = v.CustomerCode,
        FullName = v.FullName,
        Email = v.Email,
        Phone = v.Phone,
        IsActive = v.IsActive
    };
}

public sealed class OutputCustomerUpdate
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public int IdCustomer { get; set; }
    public bool Ok => ResponseCode == 0;
}

#endregion

#region Customer Select

public sealed class InputCustomerSelect { public int IdCustomer { get; set; } }

public sealed class OutputCustomerSelect
{
    public int IdCustomer { get; set; }
    public string? CustomerCode { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public InCustomerUpdateView ToUpdateView() => new()
    {
        IdCustomer = IdCustomer,
        CustomerCode = CustomerCode,
        FullName = FullName,
        Email = Email,
        Phone = Phone,
        IsActive = IsActive
    };
}

#endregion

#region Customer List

public sealed class InputCustomerList
{
    public string? Search { get; set; }
    public bool? ActiveOnly { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string SortBy { get; set; } = "fullname";
    public bool SortDesc { get; set; }
}

public sealed class OutputCustomerList
{
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<OutputCustomerSelect> Items { get; set; } = new();
}

#endregion
