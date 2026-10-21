using System.ComponentModel.DataAnnotations;
using SmartCartPro.CustomModelValidation;

namespace SmartCartPro.Models;

#region Store Create

public sealed class InStoreCreateView
{
    [Display(Name = "Store Code")]
    [Required(ErrorMessage = "The Store Code field is required.")]
    [StringLengthValidation(2, 20, ErrorMessage = "The Store Code must be between 2 and 20 characters.")]
    [NoScriptTags(ErrorMessage = "The Store Code cannot contain script tags.")]
    public string StoreCode { get; set; } = string.Empty;

    [Display(Name = "Store Name")]
    [Required(ErrorMessage = "The Store Name field is required.")]
    [TrimmedString(2, 200, ErrorMessage = "The Store Name must be between 2 and 200 characters.")]
    [NoScriptTags(ErrorMessage = "The Store Name cannot contain script tags.")]
    public string StoreName { get; set; } = string.Empty;

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

    [Display(Name = "Default Store")]
    public bool IsDefault { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}

public sealed class InputStoreCreate
{
    public int ActorUserId { get; set; }
    public bool CanCreate { get; set; }
    public int FkCompany { get; set; }

    public string StoreCode { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class OutputStoreCreate
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }

    public int IdStore { get; set; }
    public string StoreCode { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;

    public bool Ok => ResponseCode == 0;
}

#endregion

#region Store Update

public sealed class InStoreUpdateView
{
    [Display(Name = "Store")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Something went wrong/Please select a valid Store.")]
    public int IdStore { get; set; }

    [Display(Name = "Store Code")]
    [Required(ErrorMessage = "The Store Code field is required.")]
    [StringLengthValidation(2, 20, ErrorMessage = "The Store Code must be between 2 and 20 characters.")]
    [NoScriptTags(ErrorMessage = "The Store Code cannot contain script tags.")]
    public string? StoreCode { get; set; }

    [Display(Name = "Store Name")]
    [Required(ErrorMessage = "The Store Name field is required.")]
    [TrimmedString(2, 200, ErrorMessage = "The Store Name must be between 2 and 200 characters.")]
    [NoScriptTags(ErrorMessage = "The Store Name cannot contain script tags.")]
    public string? StoreName { get; set; }

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

    [Display(Name = "Default Store")]
    public bool IsDefault { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}

public sealed class InputStoreUpdate
{
    public int ActorUserId { get; set; }
    public int IdStore { get; set; }

    public string StoreCode { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class OutputStoreUpdate
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }

    public int IdStore { get; set; }
    public string StoreCode { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;

    public bool Ok => ResponseCode == 0;
}

#endregion

#region Store Select

public sealed class InputStoreSelect
{
    public int IdStore { get; set; }
}

public sealed class OutputStoreSelect
{
    public int IdStore { get; set; }
    public string StoreCode { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;

    public InStoreUpdateView ToUpdateView() => new()
    {
        IdStore = IdStore,
        StoreCode = StoreCode,
        StoreName = StoreName,
        Email = Email,
        Phone = Phone,
        AddressLine1 = AddressLine1,
        AddressLine2 = AddressLine2,
        City = City,
        State = State,
        PostalCode = PostalCode,
        Country = Country,
        IsDefault = IsDefault,
        IsActive = IsActive
    };
}

#endregion

#region Store List

public sealed class InputStoreList
{
    public bool CanCreate { get; set; }
    public string? Search { get; set; }
    public bool? ActiveOnly { get; set; }
    public string SortBy { get; set; } = "storename";
    public bool SortDesc { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public sealed class OutputStoreList
{
    public int CurrentStoreCount { get; set; }
    public int MaxStores { get; set; }
    public bool CanCreateStore { get; set; }
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<OutputStoreSelect> Stores { get; set; } = new();
    public InputStoreList Filter { get; set; } = new();
}

#endregion

#region Store Status

public sealed class InputStoreStatusUpdate
{
    public int ActorUserId { get; set; }
    public int IdStore { get; set; }
    public bool IsActive { get; set; }
}

public sealed class OutputStoreStatusUpdate
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public int IdStore { get; set; }
    public bool IsActive { get; set; }
    public bool Ok => ResponseCode == 0;
}

#endregion

#region Store Accessible List

public sealed class InputStoreAccessibleList
{
    public int IdUser { get; set; }
    public bool AllActiveStores { get; set; }
}

public sealed class OutputStoreAccessibleList
{
    public int CurrentFkStore { get; set; }
    public List<OutputStoreSelect> Stores { get; set; } = new();
}

#endregion
