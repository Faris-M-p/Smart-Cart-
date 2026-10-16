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
}
