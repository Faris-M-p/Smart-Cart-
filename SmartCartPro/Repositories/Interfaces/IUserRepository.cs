using SmartCartPro.Models;

namespace SmartCartPro.Repositories.Interfaces;

public interface IUserRepository
{
    Task<OutputUserList> ListUsersAsync(InputUserList input, CancellationToken ct = default);
    Task<OutputUserSelect?> GetUserAsync(InputUserSelect input, CancellationToken ct = default);
    Task<OutputUserOptionList> ListUserOptionsAsync(InputUserOptionList input, CancellationToken ct = default);

    /// <summary>True when any user (including the platform admin and cancelled rows) already uses this username or email, ignoring case.</summary>
    Task<bool> IsLoginIdentifierTakenAsync(string identifier, int? excludeIdUser, CancellationToken ct = default);

    Task<OutputUserCreate> CreateUserAsync(InputUserCreate input, CancellationToken ct = default);

    /// <summary>Returns null when the employee does not exist (platform super admin accounts are never touched).</summary>
    Task<OutputUserUpdate?> UpdateUserAsync(InputUserUpdate input, CancellationToken ct = default);

    /// <summary>Returns null when the employee does not exist (platform super admin accounts are never touched).</summary>
    Task<OutputUserStatusUpdate?> UpdateUserStatusAsync(InputUserStatusUpdate input, CancellationToken ct = default);

    /// <summary>Stores the new hash and forces a password change at next sign-in. Returns null when the employee does not exist.</summary>
    Task<OutputUserPasswordUpdate?> UpdateUserPasswordAsync(InputUserPasswordUpdate input, CancellationToken ct = default);

    /// <summary>Used by shared filters and the layout. Do not change the signature or behaviour.</summary>
    Task<List<OutputPermissionSelect>> GetUserPermissionsAsync(int idUser, CancellationToken ct = default);
}
