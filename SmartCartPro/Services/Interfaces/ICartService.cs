using SmartCartPro.Models;

namespace SmartCartPro.Services.Interfaces;

public interface ICartService
{
    Task<OutputCartSelect?> GetCartAsync(InputCartSelect input, CancellationToken ct = default);
    Task<OutputCartUpsertItem> UpsertItemAsync(InputCartUpsertItem input, CancellationToken ct = default);
    Task<OutputCartClear> ClearCartAsync(InputCartClear input, CancellationToken ct = default);
}

public interface IWishlistService
{
    Task<OutputWishlistSelect?> GetWishlistAsync(InputWishlistSelect input, CancellationToken ct = default);
    Task<OutputWishlistToggle> ToggleItemAsync(InputWishlistToggle input, CancellationToken ct = default);
}
