using System.ComponentModel.DataAnnotations;
using SmartCartPro.CustomModelValidation;

namespace SmartCartPro.Models;

#region Supplier Create

public sealed class InSupplierCreateView
{
    [Display(Name = "Supplier Code")]
    [StringLengthValidation(0, 20, ErrorMessage = "The Supplier Code cannot exceed 20 characters.")]
    [NoScriptTags(ErrorMessage = "The Supplier Code cannot contain script tags.")]
    public string? SupplierCode { get; set; }

    [Display(Name = "Supplier Name")]
    [Required(ErrorMessage = "The Supplier Name field is required.")]
    [TrimmedString(2, 300, ErrorMessage = "The Supplier Name must be between 2 and 300 characters.")]
    [NoScriptTags(ErrorMessage = "The Supplier Name cannot contain script tags.")]
    public string SupplierName { get; set; } = string.Empty;

    [Display(Name = "Contact Person")]
    [StringLengthValidation(0, 200, ErrorMessage = "The Contact Person cannot exceed 200 characters.")]
    [NoScriptTags(ErrorMessage = "The Contact Person cannot contain script tags.")]
    public string? ContactPerson { get; set; }

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

    [Display(Name = "Address")]
    [StringLengthValidation(0, 200, ErrorMessage = "The Address cannot exceed 200 characters.")]
    [NoScriptTags(ErrorMessage = "The Address cannot contain script tags.")]
    public string? AddressLine1 { get; set; }

    [Display(Name = "City")]
    [StringLengthValidation(0, 100, ErrorMessage = "The City cannot exceed 100 characters.")]
    [NoScriptTags(ErrorMessage = "The City cannot contain script tags.")]
    public string? City { get; set; }

    [Display(Name = "State")]
    [StringLengthValidation(0, 100, ErrorMessage = "The State cannot exceed 100 characters.")]
    [NoScriptTags(ErrorMessage = "The State cannot contain script tags.")]
    public string? State { get; set; }

    [Display(Name = "Postal Code")]
    [StringLengthValidation(0, 20, ErrorMessage = "The Postal Code cannot exceed 20 characters.")]
    [ZipCode(ErrorMessage = "Invalid postal code.")]
    public string? PostalCode { get; set; }

    [Display(Name = "Country")]
    [StringLengthValidation(0, 100, ErrorMessage = "The Country cannot exceed 100 characters.")]
    [NoScriptTags(ErrorMessage = "The Country cannot contain script tags.")]
    public string? Country { get; set; }

    [Display(Name = "Tax / GST Number")]
    [StringLengthValidation(0, 50, ErrorMessage = "The Tax / GST Number cannot exceed 50 characters.")]
    [NoScriptTags(ErrorMessage = "The Tax / GST Number cannot contain script tags.")]
    public string? TaxNumber { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}

public sealed class InputSupplierCreate
{
    public string? SupplierCode { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? AddressLine1 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? TaxNumber { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class OutputSupplierCreate
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public int IdSupplier { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public bool Ok => ResponseCode == 0;
}

#endregion

#region Supplier Update

public sealed class InSupplierUpdateView
{
    [Display(Name = "Supplier")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Something went wrong/Please select a valid Supplier.")]
    public int IdSupplier { get; set; }

    [Display(Name = "Supplier Code")]
    [StringLengthValidation(0, 20, ErrorMessage = "The Supplier Code cannot exceed 20 characters.")]
    [NoScriptTags(ErrorMessage = "The Supplier Code cannot contain script tags.")]
    public string? SupplierCode { get; set; }

    [Display(Name = "Supplier Name")]
    [Required(ErrorMessage = "The Supplier Name field is required.")]
    [TrimmedString(2, 300, ErrorMessage = "The Supplier Name must be between 2 and 300 characters.")]
    [NoScriptTags(ErrorMessage = "The Supplier Name cannot contain script tags.")]
    public string SupplierName { get; set; } = string.Empty;

    [Display(Name = "Contact Person")]
    [StringLengthValidation(0, 200, ErrorMessage = "The Contact Person cannot exceed 200 characters.")]
    [NoScriptTags(ErrorMessage = "The Contact Person cannot contain script tags.")]
    public string? ContactPerson { get; set; }

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

    [Display(Name = "Address")]
    [StringLengthValidation(0, 200, ErrorMessage = "The Address cannot exceed 200 characters.")]
    [NoScriptTags(ErrorMessage = "The Address cannot contain script tags.")]
    public string? AddressLine1 { get; set; }

    [Display(Name = "City")]
    [StringLengthValidation(0, 100, ErrorMessage = "The City cannot exceed 100 characters.")]
    [NoScriptTags(ErrorMessage = "The City cannot contain script tags.")]
    public string? City { get; set; }

    [Display(Name = "State")]
    [StringLengthValidation(0, 100, ErrorMessage = "The State cannot exceed 100 characters.")]
    [NoScriptTags(ErrorMessage = "The State cannot contain script tags.")]
    public string? State { get; set; }

    [Display(Name = "Postal Code")]
    [StringLengthValidation(0, 20, ErrorMessage = "The Postal Code cannot exceed 20 characters.")]
    [ZipCode(ErrorMessage = "Invalid postal code.")]
    public string? PostalCode { get; set; }

    [Display(Name = "Country")]
    [StringLengthValidation(0, 100, ErrorMessage = "The Country cannot exceed 100 characters.")]
    [NoScriptTags(ErrorMessage = "The Country cannot contain script tags.")]
    public string? Country { get; set; }

    [Display(Name = "Tax / GST Number")]
    [StringLengthValidation(0, 50, ErrorMessage = "The Tax / GST Number cannot exceed 50 characters.")]
    [NoScriptTags(ErrorMessage = "The Tax / GST Number cannot contain script tags.")]
    public string? TaxNumber { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}

public sealed class InputSupplierUpdate
{
    public int IdSupplier { get; set; }
    public string? SupplierCode { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? AddressLine1 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? TaxNumber { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class OutputSupplierUpdate
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public int IdSupplier { get; set; }
    public bool Ok => ResponseCode == 0;
}

#endregion

#region Supplier Set Active

public sealed class InputSupplierSetActive
{
    public int IdSupplier { get; set; }
    public bool IsActive { get; set; }
}

public sealed class OutputSupplierSetActive
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public int IdSupplier { get; set; }
    public bool IsActive { get; set; }
    public bool Ok => ResponseCode == 0;
}

#endregion

#region Supplier Select

public sealed class InputSupplierSelect { public int IdSupplier { get; set; } }

public sealed class OutputSupplierSelect
{
    public int IdSupplier { get; set; }
    public string? SupplierCode { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? AddressLine1 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? TaxNumber { get; set; }
    public bool IsActive { get; set; }

    public InSupplierUpdateView ToUpdateView() => new()
    {
        IdSupplier = IdSupplier,
        SupplierCode = SupplierCode,
        SupplierName = SupplierName,
        ContactPerson = ContactPerson,
        Email = Email,
        Phone = Phone,
        AddressLine1 = AddressLine1,
        City = City,
        State = State,
        PostalCode = PostalCode,
        Country = Country,
        TaxNumber = TaxNumber,
        IsActive = IsActive
    };
}

#endregion

#region Supplier List

public sealed class InputSupplierList
{
    public string? Search { get; set; }
    /// <summary>null = all suppliers, true = active only, false = inactive only.</summary>
    public bool? ActiveOnly { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    /// <summary>suppliercode | suppliername | city | isactive</summary>
    public string SortBy { get; set; } = "suppliername";
    public bool SortDesc { get; set; }
}

public sealed class OutputSupplierList
{
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<OutputSupplierSelect> Items { get; set; } = new();
    /// <summary>Normalised filter echoed back so the view can repopulate its form and build pager links.</summary>
    public InputSupplierList Filter { get; set; } = new();
}

#endregion
