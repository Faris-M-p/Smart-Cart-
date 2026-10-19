using System.ComponentModel.DataAnnotations;

namespace SmartCartPro.Models;

#region Supplier Create

public sealed class InSupplierCreateView
{
    [MaxLength(20)]
    [Display(Name = "Supplier code")]
    public string? SupplierCode { get; set; }

    [Required, MaxLength(300)]
    [Display(Name = "Supplier name")]
    public string SupplierName { get; set; } = string.Empty;

    [MaxLength(200)]
    [Display(Name = "Contact person")]
    public string? ContactPerson { get; set; }

    [MaxLength(200), EmailAddress]
    public string? Email { get; set; }

    [MaxLength(30)]
    public string? Phone { get; set; }

    [Display(Name = "Address")]
    public string? AddressLine1 { get; set; }

    public string? City { get; set; }
    public string? State { get; set; }

    [Display(Name = "Postal code")]
    public string? PostalCode { get; set; }

    public string? Country { get; set; }

    [Display(Name = "Tax / GST number")]
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

    public static InputSupplierCreate From(InSupplierCreateView v) => new()
    {
        SupplierCode = v.SupplierCode,
        SupplierName = v.SupplierName,
        ContactPerson = v.ContactPerson,
        Email = v.Email,
        Phone = v.Phone,
        AddressLine1 = v.AddressLine1,
        City = v.City,
        State = v.State,
        PostalCode = v.PostalCode,
        Country = v.Country,
        TaxNumber = v.TaxNumber,
        IsActive = v.IsActive
    };
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
    public int IdSupplier { get; set; }

    [MaxLength(20)]
    [Display(Name = "Supplier code")]
    public string? SupplierCode { get; set; }

    [Required, MaxLength(300)]
    [Display(Name = "Supplier name")]
    public string SupplierName { get; set; } = string.Empty;

    [MaxLength(200)]
    [Display(Name = "Contact person")]
    public string? ContactPerson { get; set; }

    [MaxLength(200), EmailAddress]
    public string? Email { get; set; }

    [MaxLength(30)]
    public string? Phone { get; set; }

    [Display(Name = "Address")]
    public string? AddressLine1 { get; set; }

    public string? City { get; set; }
    public string? State { get; set; }

    [Display(Name = "Postal code")]
    public string? PostalCode { get; set; }

    public string? Country { get; set; }

    [Display(Name = "Tax / GST number")]
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

    public static InputSupplierUpdate From(InSupplierUpdateView v) => new()
    {
        IdSupplier = v.IdSupplier,
        SupplierCode = v.SupplierCode,
        SupplierName = v.SupplierName,
        ContactPerson = v.ContactPerson,
        Email = v.Email,
        Phone = v.Phone,
        AddressLine1 = v.AddressLine1,
        City = v.City,
        State = v.State,
        PostalCode = v.PostalCode,
        Country = v.Country,
        TaxNumber = v.TaxNumber,
        IsActive = v.IsActive
    };
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
    public bool? ActiveOnly { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string SortBy { get; set; } = "suppliername";
    public bool SortDesc { get; set; }
}

public sealed class OutputSupplierList
{
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<OutputSupplierSelect> Items { get; set; } = new();
}

#endregion
