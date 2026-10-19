using SmartCartPro.Models;

namespace SmartCartPro.Services.Interfaces;

public interface IUserService
{
    Task<OutputUserList> ListUsersAsync(InputUserList input, CancellationToken ct = default);
    Task<OutputUserSelect?> GetUserAsync(InputUserSelect input, CancellationToken ct = default);
    Task<OutputUserOptionList> ListUserOptionsAsync(InputUserOptionList input, CancellationToken ct = default);
    Task<OutputUserCreate> CreateUserAsync(InputUserCreate input, CancellationToken ct = default);
    Task<OutputUserUpdate> UpdateUserAsync(InputUserUpdate input, CancellationToken ct = default);
    Task<OutputUserStatusUpdate> UpdateUserStatusAsync(InputUserStatusUpdate input, CancellationToken ct = default);
    Task<OutputUserPasswordUpdate> UpdateUserPasswordAsync(InputUserPasswordUpdate input, CancellationToken ct = default);

    /// <summary>Used by shared filters and the layout (ICurrentUserAccess). Do not change the signature or behaviour.</summary>
    Task<List<OutputPermissionSelect>> GetUserPermissionsAsync(int idUser, CancellationToken ct = default);
}
