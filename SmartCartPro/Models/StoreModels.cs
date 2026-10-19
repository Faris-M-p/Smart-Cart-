using System.ComponentModel.DataAnnotations;

namespace SmartCartPro.Models;

#region Store Create

public sealed class InStoreCreateView
{
    [Required, MaxLength(20)]
    [Display(Name = "Store code")]
    public string StoreCode { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    [Display(Name = "Store name")]
    public string StoreName { get; set; } = string.Empty;

    [MaxLength(200), EmailAddress]
    public string? Email { get; set; }

    [MaxLength(30)]
    public string? Phone { get; set; }

    [Display(Name = "Address line 1")]
    public string? AddressLine1 { get; set; }

    [Display(Name = "Address line 2")]
    public string? AddressLine2 { get; set; }

    public string? City { get; set; }
    public string? State { get; set; }

    [Display(Name = "Postal code")]
    public string? PostalCode { get; set; }

    public string? Country { get; set; }

    [Display(Name = "Default store")]
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

    public static InputStoreCreate From(InStoreCreateView view, int actorUserId, bool canCreate) => new()
    {
        ActorUserId = actorUserId,
        CanCreate = canCreate,
        StoreCode = view.StoreCode,
        StoreName = view.StoreName,
        Email = view.Email,
        Phone = view.Phone,
        AddressLine1 = view.AddressLine1,
        AddressLine2 = view.AddressLine2,
        City = view.City,
        State = view.State,
        PostalCode = view.PostalCode,
        Country = view.Country,
        IsDefault = view.IsDefault,
        IsActive = view.IsActive
    };
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
    public int IdStore { get; set; }

    [Required, MaxLength(20)]
    [Display(Name = "Store code")]
    public string StoreCode { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    [Display(Name = "Store name")]
    public string StoreName { get; set; } = string.Empty;

    [MaxLength(200), EmailAddress]
    public string? Email { get; set; }

    [MaxLength(30)]
    public string? Phone { get; set; }

    [Display(Name = "Address line 1")]
    public string? AddressLine1 { get; set; }

    [Display(Name = "Address line 2")]
    public string? AddressLine2 { get; set; }

    public string? City { get; set; }
    public string? State { get; set; }

    [Display(Name = "Postal code")]
    public string? PostalCode { get; set; }

    public string? Country { get; set; }

    [Display(Name = "Default store")]
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

    public static InputStoreUpdate From(InStoreUpdateView view, int actorUserId) => new()
    {
        ActorUserId = actorUserId,
        IdStore = view.IdStore,
        StoreCode = view.StoreCode,
        StoreName = view.StoreName,
        Email = view.Email,
        Phone = view.Phone,
        AddressLine1 = view.AddressLine1,
        AddressLine2 = view.AddressLine2,
        City = view.City,
        State = view.State,
        PostalCode = view.PostalCode,
        Country = view.Country,
        IsDefault = view.IsDefault,
        IsActive = view.IsActive
    };
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
}

public sealed class OutputStoreList
{
    public int CurrentStoreCount { get; set; }
    public int MaxStores { get; set; }
    public bool CanCreateStore { get; set; }
    public List<OutputStoreSelect> Stores { get; set; } = new();
}

#endregion
