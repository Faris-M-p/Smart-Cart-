using SmartCartPro.Models;

namespace SmartCartPro.Repositories.Interfaces;

public interface ICartRepository
{
    Task<OutputCartSelect?> GetCartAsync(InputCartSelect input, CancellationToken ct = default);
    Task<OutputCartUpsertItem> UpsertItemAsync(InputCartUpsertItem input, CancellationToken ct = default);
    Task<OutputCartClear> ClearCartAsync(InputCartClear input, CancellationToken ct = default);
}

public interface IWishlistRepository
{
    Task<OutputWishlistSelect?> GetWishlistAsync(InputWishlistSelect input, CancellationToken ct = default);
    Task<OutputWishlistToggle> ToggleItemAsync(InputWishlistToggle input, CancellationToken ct = default);
}
