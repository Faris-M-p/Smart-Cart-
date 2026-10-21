using SmartCartPro.Entities;
using SmartCartPro.Models;

namespace SmartCartPro.Repositories.Interfaces;

public interface IAuthRepository
{
    Task<User?> FindActiveByUserNameOrEmailAsync(string userNameOrEmail, CancellationToken cancellationToken = default);

    Task<User?> GetByIdAsync(int idUser, CancellationToken cancellationToken = default);

    Task UpdateUserAsync(User user, CancellationToken cancellationToken = default);

    Task<int?> GetSessionVersionAsync(int idUser, CancellationToken cancellationToken = default);

    Task<AuthUserStoreInfo?> GetPreferredStoreAsync(int idUser, CancellationToken cancellationToken = default);

    Task<AuthUserStoreInfo?> GetStoreAccessAsync(int idUser, int fkStore, CancellationToken cancellationToken = default);

    Task<AuthUserStoreInfo?> GetAnyActiveStoreAsync(CancellationToken cancellationToken = default);

    Task<AuthUserStoreInfo?> GetActiveStoreByIdAsync(int fkStore, CancellationToken cancellationToken = default);

    Task AddOtpChallengeAsync(AuthOtpChallenge challenge, CancellationToken cancellationToken = default);

    Task<AuthOtpChallenge?> GetOtpChallengeAsync(string challengeToken, CancellationToken cancellationToken = default);

    Task UpdateOtpChallengeAsync(AuthOtpChallenge challenge, CancellationToken cancellationToken = default);

    Task InvalidateOpenOtpChallengesAsync(int idUser, CancellationToken cancellationToken = default);
}
