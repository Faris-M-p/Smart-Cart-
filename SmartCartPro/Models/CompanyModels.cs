using System.ComponentModel.DataAnnotations;
using SmartCartPro.CustomModelValidation;

namespace SmartCartPro.Models;

#region Company Create

public sealed class InputCompanyCreate
{
    public int ActorUserId { get; set; }
    public bool CanCreate { get; set; }

    public string CompanyName { get; set; } = string.Empty;
    public string CompanyCode { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? TaxNumber { get; set; }
    public string? RegistrationNo { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string CurrencyCode { get; set; } = "INR";
    public string TimeZone { get; set; } = "Asia/Kolkata";
    public string Locale { get; set; } = "en-IN";
    public bool IsActive { get; set; } = true;
}

public sealed class OutputCompanyCreate
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }

    public int IdCompany { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string CompanyCode { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? TaxNumber { get; set; }
    public string? RegistrationNo { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string CurrencyCode { get; set; } = "INR";
    public string TimeZone { get; set; } = "Asia/Kolkata";
    public string Locale { get; set; } = "en-IN";
    public bool IsActive { get; set; } = true;

    public bool Ok => ResponseCode == 0;
}

#endregion

#region Company Update

public sealed class InCompanyUpdateView
{
    [Display(Name = "Company")]
    [GreaterThanOrEqualToZero(ErrorMessage = "Something went wrong/Please select a valid Company.")]
    public int IdCompany { get; set; }

    [Display(Name = "Company Name")]
    [Required(ErrorMessage = "The Company Name field is required.")]
    [TrimmedString(2, 200, ErrorMessage = "The Company Name must be between 2 and 200 characters.")]
    [NoScriptTags(ErrorMessage = "The Company Name cannot contain script tags.")]
    public string CompanyName { get; set; } = string.Empty;

    [Display(Name = "Company Code")]
    [Required(ErrorMessage = "The Company Code field is required.")]
    [StringLengthValidation(2, 20, ErrorMessage = "The Company Code must be between 2 and 20 characters.")]
    [NoScriptTags(ErrorMessage = "The Company Code cannot contain script tags.")]
    public string CompanyCode { get; set; } = string.Empty;

    [Display(Name = "Legal Name")]
    [StringLengthValidation(0, 200, ErrorMessage = "The Legal Name cannot exceed 200 characters.")]
    [NoScriptTags(ErrorMessage = "The Legal Name cannot contain script tags.")]
    public string? LegalName { get; set; }

    [Display(Name = "Tax Number")]
    [StringLengthValidation(0, 50, ErrorMessage = "The Tax Number cannot exceed 50 characters.")]
    [NoScriptTags(ErrorMessage = "The Tax Number cannot contain script tags.")]
    public string? TaxNumber { get; set; }

    [Display(Name = "Registration No.")]
    [StringLengthValidation(0, 50, ErrorMessage = "The Registration No. cannot exceed 50 characters.")]
    [NoScriptTags(ErrorMessage = "The Registration No. cannot contain script tags.")]
    public string? RegistrationNo { get; set; }

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

    [Display(Name = "Website")]
    [StringLengthValidation(0, 200, ErrorMessage = "The Website cannot exceed 200 characters.")]
    [NoScriptTags(ErrorMessage = "The Website cannot contain script tags.")]
    public string? Website { get; set; }

    [Display(Name = "Address Line 1")]
    [StringLengthValidation(0, 200, ErrorMessage = "The Address Line 1 cannot exceed 200 characters.")]
    [NoScriptTags(ErrorMessage = "The Address Line 1 cannot contain script tags.")]
    public string? AddressLine1 { get; set; }

    [Display(Name = "Address Line 2")]
    [StringLengthValidation(0, 200, ErrorMessage = "The Address Line 2 cannot exceed 200 characters.")]
    [NoScriptTags(ErrorMessage = "The Address Line 2 cannot contain script tags.")]
    public string? AddressLine2 { get; set; }

    [Display(Name = "City")]
    [StringLengthValidation(0, 100, ErrorMessage = "The City cannot exceed 100 characters.")]
    [NoScriptTags(ErrorMessage = "The City cannot contain script tags.")]
    public string? City { get; set; }

    [Display(Name = "State")]
    [StringLengthValidation(0, 100, ErrorMessage = "The State cannot exceed 100 characters.")]
    [NoScriptTags(ErrorMessage = "The State cannot contain script tags.")]
    public string? State { get; set; }

    [Display(Name = "Postal Code")]
    [ZipCode(ErrorMessage = "Invalid postal code.")]
    public string? PostalCode { get; set; }

    [Display(Name = "Country")]
    [StringLengthValidation(0, 100, ErrorMessage = "The Country cannot exceed 100 characters.")]
    [NoScriptTags(ErrorMessage = "The Country cannot contain script tags.")]
    public string? Country { get; set; }

    [Display(Name = "Currency Code")]
    [Required(ErrorMessage = "The Currency Code field is required.")]
    [StringLengthValidation(3, 3, ErrorMessage = "The Currency Code must be 3 characters.")]
    [NoScriptTags(ErrorMessage = "The Currency Code cannot contain script tags.")]
    public string CurrencyCode { get; set; } = "INR";

    [Display(Name = "Time Zone")]
    [Required(ErrorMessage = "The Time Zone field is required.")]
    [StringLengthValidation(2, 100, ErrorMessage = "The Time Zone must be between 2 and 100 characters.")]
    [NoScriptTags(ErrorMessage = "The Time Zone cannot contain script tags.")]
    public string TimeZone { get; set; } = "Asia/Kolkata";

    [Display(Name = "Locale")]
    [Required(ErrorMessage = "The Locale field is required.")]
    [StringLengthValidation(2, 20, ErrorMessage = "The Locale must be between 2 and 20 characters.")]
    [NoScriptTags(ErrorMessage = "The Locale cannot contain script tags.")]
    public string Locale { get; set; } = "en-IN";

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Can Edit")]
    public bool CanEdit { get; set; }

    [Display(Name = "Requires Bootstrap")]
    public bool RequiresBootstrap { get; set; }
}

public sealed class InputCompanyUpdate
{
    public int ActorUserId { get; set; }
    public int IdCompany { get; set; }

    public string CompanyName { get; set; } = string.Empty;
    public string CompanyCode { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? TaxNumber { get; set; }
    public string? RegistrationNo { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string CurrencyCode { get; set; } = "INR";
    public string TimeZone { get; set; } = "Asia/Kolkata";
    public string Locale { get; set; } = "en-IN";
    public bool IsActive { get; set; } = true;
}

public sealed class OutputCompanyUpdate
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }

    public int IdCompany { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string CompanyCode { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? TaxNumber { get; set; }
    public string? RegistrationNo { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string CurrencyCode { get; set; } = "INR";
    public string TimeZone { get; set; } = "Asia/Kolkata";
    public string Locale { get; set; } = "en-IN";
    public bool IsActive { get; set; } = true;

    public bool Ok => ResponseCode == 0;
}

#endregion

#region Company Select

public sealed class InputCompanySelect
{
    public bool CanCreate { get; set; }
}

public sealed class OutputCompanySelect
{
    public int IdCompany { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string CompanyCode { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? TaxNumber { get; set; }
    public string? RegistrationNo { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string CurrencyCode { get; set; } = "INR";
    public string TimeZone { get; set; } = "Asia/Kolkata";
    public string Locale { get; set; } = "en-IN";
    public bool IsActive { get; set; } = true;
    public bool CanEdit { get; set; }
    public bool RequiresBootstrap { get; set; }

    public InCompanyUpdateView ToView() => new()
    {
        IdCompany = IdCompany,
        CompanyName = CompanyName,
        CompanyCode = CompanyCode,
        LegalName = LegalName,
        TaxNumber = TaxNumber,
        RegistrationNo = RegistrationNo,
        Email = Email,
        Phone = Phone,
        Website = Website,
        AddressLine1 = AddressLine1,
        AddressLine2 = AddressLine2,
        City = City,
        State = State,
        PostalCode = PostalCode,
        Country = Country,
        CurrencyCode = CurrencyCode,
        TimeZone = TimeZone,
        Locale = Locale,
        IsActive = IsActive,
        CanEdit = CanEdit,
        RequiresBootstrap = RequiresBootstrap
    };
}

#endregion
