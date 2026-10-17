using Microsoft.EntityFrameworkCore;
using SmartCartPro.Data;
using SmartCartPro.Entities;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;

namespace SmartCartPro.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly SmartCartDbContext _db;

    public AuthRepository(SmartCartDbContext db)
    {
        _db = db;
    }

    public async Task<User?> FindActiveByUserNameOrEmailAsync(
        string userNameOrEmail,
        CancellationToken cancellationToken = default)
    {
        var login = userNameOrEmail.Trim().ToLowerInvariant();
        return await _db.Users
            .FirstOrDefaultAsync(
                u => !u.Cancelled
                     && u.IsActive
                     && (u.UserName.ToLower() == login || u.Email.ToLower() == login),
                cancellationToken);
    }

    public Task<User?> GetByIdAsync(int idUser, CancellationToken cancellationToken = default) =>
        _db.Users.FirstOrDefaultAsync(
            u => u.IdUser == idUser && !u.Cancelled && u.IsActive,
            cancellationToken);

    public async Task UpdateUserAsync(User user, CancellationToken cancellationToken = default)
    {
        user.UpdatedAt = DateTime.UtcNow;
        if (_db.Entry(user).State == EntityState.Detached)
        {
            _db.Users.Attach(user);
            _db.Entry(user).State = EntityState.Modified;
        }
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<AuthUserStoreInfo?> GetPreferredStoreAsync(
        int idUser,
        CancellationToken cancellationToken = default)
    {
        var query = from a in _db.UserStoreAccess.AsNoTracking()
                    join s in _db.Stores.AsNoTracking() on a.FkStore equals s.IdStore
                    where a.FkUser == idUser
                          && !a.Cancelled
                          && !s.Cancelled
                          && s.IsActive
                    orderby a.IsDefault descending, s.StoreName
                    select new AuthUserStoreInfo
                    {
                        Fk_Store = s.IdStore,
                        Fk_Company = s.FkCompany,
                        StoreCode = s.StoreCode,
                        StoreName = s.StoreName,
                        IsDefault = a.IsDefault
                    };

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<AuthUserStoreInfo?> GetStoreAccessAsync(
        int idUser,
        int fkStore,
        CancellationToken cancellationToken = default)
    {
        return await (
            from a in _db.UserStoreAccess.AsNoTracking()
            join s in _db.Stores.AsNoTracking() on a.FkStore equals s.IdStore
            where a.FkUser == idUser
                  && a.FkStore == fkStore
                  && !a.Cancelled
                  && !s.Cancelled
                  && s.IsActive
            select new AuthUserStoreInfo
            {
                Fk_Store = s.IdStore,
                Fk_Company = s.FkCompany,
                StoreCode = s.StoreCode,
                StoreName = s.StoreName,
                IsDefault = a.IsDefault
            }).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddOtpChallengeAsync(AuthOtpChallenge challenge, CancellationToken cancellationToken = default)
    {
        _db.AuthOtpChallenges.Add(challenge);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<AuthOtpChallenge?> GetOtpChallengeAsync(
        string challengeToken,
        CancellationToken cancellationToken = default) =>
        _db.AuthOtpChallenges.FirstOrDefaultAsync(
            c => c.ChallengeToken == challengeToken,
            cancellationToken);

    public async Task UpdateOtpChallengeAsync(AuthOtpChallenge challenge, CancellationToken cancellationToken = default)
    {
        if (_db.Entry(challenge).State == EntityState.Detached)
        {
            _db.AuthOtpChallenges.Attach(challenge);
            _db.Entry(challenge).State = EntityState.Modified;
        }
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task InvalidateOpenOtpChallengesAsync(int idUser, CancellationToken cancellationToken = default)
    {
        var open = await _db.AuthOtpChallenges
            .Where(c => c.FkUser == idUser && c.ConsumedAt == null && c.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        if (open.Count == 0)
        {
            return;
        }

        var now = DateTime.UtcNow;
        foreach (var item in open)
        {
            item.ExpiresAt = now;
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
