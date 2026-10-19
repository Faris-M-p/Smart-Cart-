using Microsoft.AspNetCore.Identity;
using SmartCartPro.Entities;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repo;
    private readonly IPasswordHasher<User> _hasher;

    public UserService(IUserRepository repo, IPasswordHasher<User> hasher)
    {
        _repo = repo;
        _hasher = hasher;
    }

    public Task<OutputUserList> ListUsersAsync(InputUserList input, CancellationToken ct = default) =>
        _repo.ListUsersAsync(input, ct);

    public Task<OutputUserSelect?> GetUserAsync(InputUserSelect input, CancellationToken ct = default) =>
        _repo.GetUserAsync(input, ct);

    public Task<OutputUserCreate> CreateUserAsync(
        InputUserCreate input,
        string plainPassword,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(plainPassword) || plainPassword.Length < 8)
        {
            return Task.FromResult(new OutputUserCreate
            {
                ResponseCode = 1,
                ResponseMsg = "Password must be at least 8 characters.",
                StatusCode = 400
            });
        }

        // Hash using a dummy user carrier (PasswordHasher is stateless on the object)
        input.PasswordHash = _hasher.HashPassword(new User(), plainPassword);
        return _repo.CreateUserAsync(input, ct);
    }

    public Task<OutputUserUpdate> UpdateUserAsync(InputUserUpdate input, CancellationToken ct = default) =>
        _repo.UpdateUserAsync(input, ct);

    public Task<bool> DeactivateUserAsync(int idUser, CancellationToken ct = default) =>
        _repo.DeactivateUserAsync(idUser, ct);

    public Task<bool> ResetPasswordAsync(int idUser, string newPlainPassword, CancellationToken ct = default)
    {
        var hash = _hasher.HashPassword(new User(), newPlainPassword);
        return _repo.ResetPasswordHashAsync(idUser, hash, ct);
    }

    public Task<bool> AssignStoresToUserAsync(int idUser, List<int> storeIds, CancellationToken ct = default) =>
        _repo.AssignStoresToUserAsync(idUser, storeIds, ct);

    public Task<bool> AssignRoleToUserAsync(int idUser, int idRole, CancellationToken ct = default) =>
        _repo.AssignRoleToUserAsync(idUser, idRole, ct);

    public Task<List<OutputPermissionSelect>> GetUserPermissionsAsync(int idUser, CancellationToken ct = default) =>
        _repo.GetUserPermissionsAsync(idUser, ct);
}
