using SmartCartPro.Entities;
using SmartCartPro.Models;

namespace SmartCartPro.Repositories.Interfaces;

public interface IAuthRepository
{
    Task<User?> FindActiveByUserNameOrEmailAsync(string userNameOrEmail, CancellationToken cancellationToken = default);

    Task<User?> GetByIdAsync(int idUser, CancellationToken cancellationToken = default);

    Task UpdateUserAsync(User user, CancellationToken cancellationToken = default);

    Task<AuthUserStoreInfo?> GetPreferredStoreAsync(int idUser, CancellationToken cancellationToken = default);

    Task<AuthUserStoreInfo?> GetStoreAccessAsync(int idUser, int fkStore, CancellationToken cancellationToken = default);

    Task<bool> AnyCompanyExistsAsync(CancellationToken cancellationToken = default);

    Task<bool> IsUserNameOrEmailTakenAsync(string userName, string email, CancellationToken cancellationToken = default);

    Task<int?> GetRoleIdByCodeAsync(string roleCode, CancellationToken cancellationToken = default);

    Task CreateBootstrapAdminAsync(
        Company company,
        Store store,
        User user,
        UserStoreAccess storeAccess,
        UserRole? userRole,
        CancellationToken cancellationToken = default);
}
