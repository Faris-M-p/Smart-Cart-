using SmartCartPro.Models;

namespace SmartCartPro.Repositories.Interfaces;

public interface IUserRepository
{
    Task<OutputUserList> ListUsersAsync(InputUserList input, CancellationToken ct = default);
    Task<OutputUserSelect?> GetUserAsync(InputUserSelect input, CancellationToken ct = default);
    Task<OutputUserCreate> CreateUserAsync(InputUserCreate input, CancellationToken ct = default);
    Task<OutputUserUpdate> UpdateUserAsync(InputUserUpdate input, CancellationToken ct = default);
    Task<bool> DeactivateUserAsync(int idUser, CancellationToken ct = default);
    Task<bool> ResetPasswordHashAsync(int idUser, string newPasswordHash, CancellationToken ct = default);
    Task<bool> AssignStoresToUserAsync(int idUser, List<int> storeIds, CancellationToken ct = default);
    Task<bool> AssignRoleToUserAsync(int idUser, int idRole, CancellationToken ct = default);
    Task<List<OutputPermissionSelect>> GetUserPermissionsAsync(int idUser, CancellationToken ct = default);
}
