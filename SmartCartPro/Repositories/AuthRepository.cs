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
        _db.Users.Update(user);
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

    public Task<bool> AnyCompanyExistsAsync(CancellationToken cancellationToken = default) =>
        _db.Companies.AsNoTracking().AnyAsync(c => !c.Cancelled, cancellationToken);

    public async Task<bool> IsUserNameOrEmailTakenAsync(
        string userName,
        string email,
        CancellationToken cancellationToken = default)
    {
        var u = userName.Trim().ToLowerInvariant();
        var e = email.Trim().ToLowerInvariant();
        return await _db.Users.AsNoTracking().AnyAsync(
            x => !x.Cancelled && (x.UserName.ToLower() == u || x.Email.ToLower() == e),
            cancellationToken);
    }

    public async Task<int?> GetRoleIdByCodeAsync(string roleCode, CancellationToken cancellationToken = default)
    {
        var id = await _db.Roles.AsNoTracking()
            .Where(r => !r.Cancelled && r.IsActive && r.RoleCode == roleCode)
            .Select(r => (int?)r.IdRole)
            .FirstOrDefaultAsync(cancellationToken);
        return id;
    }

    public async Task CreateBootstrapAdminAsync(
        Company company,
        Store store,
        User user,
        UserStoreAccess storeAccess,
        UserRole? userRole,
        CancellationToken cancellationToken = default)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);

        _db.Companies.Add(company);
        await _db.SaveChangesAsync(cancellationToken);

        store.FkCompany = company.IdCompany;
        _db.Stores.Add(store);
        await _db.SaveChangesAsync(cancellationToken);

        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);

        storeAccess.FkUser = user.IdUser;
        storeAccess.FkStore = store.IdStore;
        _db.UserStoreAccess.Add(storeAccess);

        if (userRole is not null)
        {
            userRole.FkUser = user.IdUser;
            _db.UserRoles.Add(userRole);
        }

        await _db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
    }
}
