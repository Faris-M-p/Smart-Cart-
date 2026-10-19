using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _repo;

    public CartService(ICartRepository repo) => _repo = repo;

    public Task<OutputCartSelect?> GetCartAsync(InputCartSelect input, CancellationToken ct = default) =>
        _repo.GetCartAsync(input, ct);

    public Task<OutputCartUpsertItem> UpsertItemAsync(InputCartUpsertItem input, CancellationToken ct = default) =>
        _repo.UpsertItemAsync(input, ct);

    public Task<OutputCartClear> ClearCartAsync(InputCartClear input, CancellationToken ct = default) =>
        _repo.ClearCartAsync(input, ct);
}

public class WishlistService : IWishlistService
{
    private readonly IWishlistRepository _repo;

    public WishlistService(IWishlistRepository repo) => _repo = repo;

    public Task<OutputWishlistSelect?> GetWishlistAsync(InputWishlistSelect input, CancellationToken ct = default) =>
        _repo.GetWishlistAsync(input, ct);

    public Task<OutputWishlistToggle> ToggleItemAsync(InputWishlistToggle input, CancellationToken ct = default) =>
        _repo.ToggleItemAsync(input, ct);
}
