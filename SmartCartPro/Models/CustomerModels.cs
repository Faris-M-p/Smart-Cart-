using System.ComponentModel.DataAnnotations;
using SmartCartPro.CustomModelValidation;

namespace SmartCartPro.Models;

#region Customer Create

public sealed class InCustomerCreateView
{
    [Display(Name = "Customer Code")]
    [StringLengthValidation(0, 20, ErrorMessage = "The Customer Code cannot exceed 20 characters.")]
    [NoScriptTags(ErrorMessage = "The Customer Code cannot contain script tags.")]
    public string? CustomerCode { get; set; }

    [Display(Name = "Full Name")]
    [Required(ErrorMessage = "The Full Name field is required.")]
    [TrimmedString(2, 300, ErrorMessage = "The Full Name must be between 2 and 300 characters.")]
    [NoScriptTags(ErrorMessage = "The Full Name cannot contain script tags.")]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "Email")]
    [EmailIfNotEmpty]
    [MatchRegexAllowNull(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "The Email format is invalid.")]
    [NoScriptTags(ErrorMessage = "The Email cannot contain script tags.")]
    public string? Email { get; set; }

    [Display(Name = "Phone")]
    [StringLength(15, ErrorMessage = "The Phone cannot exceed 15 characters.")]
    [PhoneIfNotEmpty]
    [PhoneNumber(ErrorMessage = "Invalid phone number format.")]
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
    [Display(Name = "Customer")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Something went wrong/Please select a valid Customer.")]
    public int IdCustomer { get; set; }

    [Display(Name = "Customer Code")]
    [StringLengthValidation(0, 20, ErrorMessage = "The Customer Code cannot exceed 20 characters.")]
    [NoScriptTags(ErrorMessage = "The Customer Code cannot contain script tags.")]
    public string? CustomerCode { get; set; }

    [Display(Name = "Full Name")]
    [Required(ErrorMessage = "The Full Name field is required.")]
    [TrimmedString(2, 300, ErrorMessage = "The Full Name must be between 2 and 300 characters.")]
    [NoScriptTags(ErrorMessage = "The Full Name cannot contain script tags.")]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "Email")]
    [EmailIfNotEmpty]
    [MatchRegexAllowNull(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "The Email format is invalid.")]
    [NoScriptTags(ErrorMessage = "The Email cannot contain script tags.")]
    public string? Email { get; set; }

    [Display(Name = "Phone")]
    [StringLength(15, ErrorMessage = "The Phone cannot exceed 15 characters.")]
    [PhoneIfNotEmpty]
    [PhoneNumber(ErrorMessage = "Invalid phone number format.")]
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
