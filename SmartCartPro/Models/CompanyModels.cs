using System.ComponentModel.DataAnnotations;

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

    public static InputCompanyCreate From(InCompanyUpdateView view, int actorUserId, bool canCreate) => new()
    {
        ActorUserId = actorUserId,
        CanCreate = canCreate,
        CompanyName = view.CompanyName,
        CompanyCode = view.CompanyCode,
        LegalName = view.LegalName,
        TaxNumber = view.TaxNumber,
        RegistrationNo = view.RegistrationNo,
        Email = view.Email,
        Phone = view.Phone,
        Website = view.Website,
        AddressLine1 = view.AddressLine1,
        AddressLine2 = view.AddressLine2,
        City = view.City,
        State = view.State,
        PostalCode = view.PostalCode,
        Country = view.Country,
        CurrencyCode = view.CurrencyCode,
        TimeZone = view.TimeZone,
        Locale = view.Locale,
        IsActive = view.IsActive
    };
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
    public int IdCompany { get; set; }

    [Required, MaxLength(200)]
    [Display(Name = "Company name")]
    public string CompanyName { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    [Display(Name = "Company code")]
    public string CompanyCode { get; set; } = string.Empty;

    [MaxLength(200)]
    [Display(Name = "Legal name")]
    public string? LegalName { get; set; }

    [MaxLength(50)]
    [Display(Name = "Tax number")]
    public string? TaxNumber { get; set; }

    [MaxLength(50)]
    [Display(Name = "Registration no.")]
    public string? RegistrationNo { get; set; }

    [MaxLength(200), EmailAddress]
    public string? Email { get; set; }

    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(200)]
    public string? Website { get; set; }

    [Display(Name = "Address line 1")]
    public string? AddressLine1 { get; set; }

    [Display(Name = "Address line 2")]
    public string? AddressLine2 { get; set; }

    public string? City { get; set; }
    public string? State { get; set; }

    [Display(Name = "Postal code")]
    public string? PostalCode { get; set; }

    public string? Country { get; set; }

    [Required, MaxLength(3)]
    [Display(Name = "Currency code")]
    public string CurrencyCode { get; set; } = "INR";

    [Required]
    [Display(Name = "Time zone")]
    public string TimeZone { get; set; } = "Asia/Kolkata";

    [Required]
    public string Locale { get; set; } = "en-IN";

    public bool IsActive { get; set; } = true;

    public bool CanEdit { get; set; }
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

    public static InputCompanyUpdate From(InCompanyUpdateView view, int actorUserId) => new()
    {
        ActorUserId = actorUserId,
        IdCompany = view.IdCompany,
        CompanyName = view.CompanyName,
        CompanyCode = view.CompanyCode,
        LegalName = view.LegalName,
        TaxNumber = view.TaxNumber,
        RegistrationNo = view.RegistrationNo,
        Email = view.Email,
        Phone = view.Phone,
        Website = view.Website,
        AddressLine1 = view.AddressLine1,
        AddressLine2 = view.AddressLine2,
        City = view.City,
        State = view.State,
        PostalCode = view.PostalCode,
        Country = view.Country,
        CurrencyCode = view.CurrencyCode,
        TimeZone = view.TimeZone,
        Locale = view.Locale,
        IsActive = view.IsActive
    };
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
