using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using SmartCartPro.CustomModelValidation;
using SmartCartPro.Exceptions;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services.Interfaces;
using DataAnnotations = System.ComponentModel.DataAnnotations;

namespace SmartCartPro.Services;

/// <summary>
/// Employee management rules: unique login identifiers, valid role and stores, password policy,
/// self-protection (no self-deactivation, no self role change, no self reset) and audit trail.
/// Platform super admin accounts are never reachable through this service.
/// </summary>
public class UserService : IUserService
{
    private const int TemporaryPasswordLength = 14;
    private const string FieldSummary = "Please correct the highlighted fields.";

    private readonly IUserRepository _repo;
    private readonly IPasswordHasher<object> _hasher;
    private readonly IAuditService _audit;

    public UserService(IUserRepository repo, IPasswordHasher<object> hasher, IAuditService audit)
    {
        _repo = repo;
        _hasher = hasher;
        _audit = audit;
    }

    public async Task<OutputUserList> ListUsersAsync(InputUserList input, CancellationToken ct = default)
    {
        input.Page = Math.Max(1, input.Page);
        input.PageSize = input.PageSize <= 0 ? 25 : Math.Min(input.PageSize, 100);
        input.Search = string.IsNullOrWhiteSpace(input.Search) ? null : input.Search.Trim();
        input.SortBy = string.IsNullOrWhiteSpace(input.SortBy) ? "fullname" : input.SortBy.Trim().ToLowerInvariant();
        if (input.FkRole <= 0) input.FkRole = null;
        if (input.FkStore <= 0) input.FkStore = null;

        var result = await _repo.ListUsersAsync(input, ct);

        // The requested page no longer exists (for example after filtering): show the last page instead.
        if (result.Users.Count == 0 && result.TotalCount > 0 && input.Page > 1)
        {
            input.Page = (int)Math.Ceiling(result.TotalCount / (double)input.PageSize);
            result = await _repo.ListUsersAsync(input, ct);
        }

        result.Filter = input;
        return result;
    }

    public Task<OutputUserSelect?> GetUserAsync(InputUserSelect input, CancellationToken ct = default) =>
        _repo.GetUserAsync(input, ct);

    public Task<OutputUserOptionList> ListUserOptionsAsync(InputUserOptionList input, CancellationToken ct = default) =>
        _repo.ListUserOptionsAsync(input, ct);

    public async Task<OutputUserCreate> CreateUserAsync(InputUserCreate input, CancellationToken ct = default)
    {
        var userName = input.UserName.Trim();
        var email = input.Email.Trim().ToLowerInvariant();
        var errors = new Dictionary<string, string[]>();

        // Login accepts username or email, so both must be unique across ALL accounts, ignoring case.
        if (await _repo.IsLoginIdentifierTakenAsync(userName, null, ct))
        {
            AddError(errors, "UserName", "This username is already in use.");
        }

        if (await _repo.IsLoginIdentifierTakenAsync(email, null, ct))
        {
            AddError(errors, "Email", "This email address is already in use.");
        }

        var options = await _repo.ListUserOptionsAsync(new InputUserOptionList { FkCompany = input.ActorFkCompany }, ct);
        var storeIds = input.StoreIds.Where(id => id > 0).Distinct().ToList();
        ValidateAssignments(options, input.FkRole, storeIds, null, new List<int>(), errors);

        string plainPassword;
        string? temporaryPassword = null;
        if (input.GeneratePassword)
        {
            plainPassword = GenerateTemporaryPassword();
            temporaryPassword = plainPassword;
        }
        else
        {
            plainPassword = input.InitialPassword ?? string.Empty;
            var passwordError = PasswordPolicyError(plainPassword, "Initial Password");
            if (passwordError is not null)
            {
                AddError(errors, "InitialPassword", passwordError);
            }
        }

        ThrowIfInvalid(errors);

        input.UserName = userName;
        input.Email = email;
        input.StoreIds = storeIds;
        input.DefaultStoreId = ResolveDefaultStore(input.DefaultStoreId, storeIds);
        input.MustChangePassword = input.GeneratePassword || input.MustChangePassword;
        input.PasswordHash = _hasher.HashPassword(new object(), plainPassword);
        input.InitialPassword = null;

        var created = await _repo.CreateUserAsync(input, ct);
        created.TemporaryPassword = temporaryPassword;

        var role = options.Roles.First(r => r.IdRole == input.FkRole);
        await AuditAsync(input.ActorUserId, created.IdUser, "Create",
            $"Employee '{created.UserName}' created with role {role.RoleCode} and {storeIds.Count} store(s).", ct);

        return created;
    }

    public async Task<OutputUserUpdate> UpdateUserAsync(InputUserUpdate input, CancellationToken ct = default)
    {
        var selectInput = new InputUserSelect { IdUser = input.IdUser, ActorFkCompany = input.ActorFkCompany };
        var existing = await _repo.GetUserAsync(selectInput, ct)
            ?? throw new ResourceNotFoundException("Employee not found.", $"IdUser={input.IdUser}");

        var email = input.Email.Trim().ToLowerInvariant();
        var errors = new Dictionary<string, string[]>();

        if (!string.Equals(email, existing.Email, StringComparison.OrdinalIgnoreCase)
            && await _repo.IsLoginIdentifierTakenAsync(email, input.IdUser, ct))
        {
            AddError(errors, "Email", "This email address is already in use.");
        }

        if (input.IdUser == input.ActorUserId && input.FkRole != (existing.CurrentRoleId ?? 0))
        {
            AddError(errors, "FkRole", "You cannot change your own role. Ask another administrator.");
        }

        var options = await _repo.ListUserOptionsAsync(new InputUserOptionList { FkCompany = input.ActorFkCompany }, ct);
        var storeIds = input.StoreIds.Where(id => id > 0).Distinct().ToList();
        ValidateAssignments(
            options, input.FkRole, storeIds, existing.CurrentRoleId,
            existing.Stores.Select(s => s.IdStore).ToList(), errors);

        ThrowIfInvalid(errors);

        input.Email = email;
        input.StoreIds = storeIds;
        input.DefaultStoreId = ResolveDefaultStore(input.DefaultStoreId, storeIds);

        var updated = await _repo.UpdateUserAsync(input, ct)
            ?? throw new ResourceNotFoundException("Employee not found.", $"IdUser={input.IdUser}");

        var role = options.Roles.First(r => r.IdRole == input.FkRole);
        await AuditAsync(input.ActorUserId, updated.IdUser, "Update",
            $"Employee '{existing.UserName}' updated (role {role.RoleCode}, {storeIds.Count} store(s)).", ct);

        return updated;
    }

    public async Task<OutputUserStatusUpdate> UpdateUserStatusAsync(InputUserStatusUpdate input, CancellationToken ct = default)
    {
        var selectInput = new InputUserSelect { IdUser = input.IdUser, ActorFkCompany = input.ActorFkCompany };
        var existing = await _repo.GetUserAsync(selectInput, ct)
            ?? throw new ResourceNotFoundException("Employee not found.", $"IdUser={input.IdUser}");

        if (!input.IsActive && input.IdUser == input.ActorUserId)
        {
            throw new ConflictException(
                "You cannot deactivate your own account.",
                $"Actor {input.ActorUserId} tried to deactivate self.");
        }

        if (existing.IsActive == input.IsActive)
        {
            return new OutputUserStatusUpdate { IdUser = existing.IdUser, IsActive = existing.IsActive };
        }

        var updated = await _repo.UpdateUserStatusAsync(input, ct)
            ?? throw new ResourceNotFoundException("Employee not found.", $"IdUser={input.IdUser}");

        await AuditAsync(input.ActorUserId, updated.IdUser, input.IsActive ? "Activate" : "Deactivate",
            $"Employee '{existing.UserName}' {(input.IsActive ? "activated" : "deactivated")}.", ct);

        return updated;
    }

    public async Task<OutputUserPasswordUpdate> UpdateUserPasswordAsync(InputUserPasswordUpdate input, CancellationToken ct = default)
    {
        if (input.IdUser == input.ActorUserId)
        {
            throw new ConflictException(
                "You cannot reset your own password here. Use the Change password page instead.",
                $"Actor {input.ActorUserId} tried to reset own password.");
        }

        var selectInput = new InputUserSelect { IdUser = input.IdUser, ActorFkCompany = input.ActorFkCompany };
        var existing = await _repo.GetUserAsync(selectInput, ct)
            ?? throw new ResourceNotFoundException("Employee not found.", $"IdUser={input.IdUser}");

        string plainPassword;
        string? temporaryPassword = null;
        if (input.GeneratePassword)
        {
            plainPassword = GenerateTemporaryPassword();
            temporaryPassword = plainPassword;
        }
        else
        {
            plainPassword = input.NewPassword ?? string.Empty;
            var passwordError = PasswordPolicyError(plainPassword, "New Password");
            if (passwordError is not null)
            {
                var errors = new Dictionary<string, string[]>();
                AddError(errors, "NewPassword", passwordError);
                ThrowIfInvalid(errors);
            }
        }

        input.PasswordHash = _hasher.HashPassword(new object(), plainPassword);
        input.NewPassword = null;

        var updated = await _repo.UpdateUserPasswordAsync(input, ct)
            ?? throw new ResourceNotFoundException("Employee not found.", $"IdUser={input.IdUser}");
        updated.TemporaryPassword = temporaryPassword;

        // Never log the password or its hash.
        await AuditAsync(input.ActorUserId, updated.IdUser, "ResetPassword",
            $"Password reset by an administrator for '{existing.UserName}'. A password change is required at next sign-in.", ct);

        return updated;
    }

    public Task<List<OutputPermissionSelect>> GetUserPermissionsAsync(int idUser, CancellationToken ct = default) =>
        _repo.GetUserPermissionsAsync(idUser, ct);

    // ------------------------------------------------------------------ helpers

    /// <summary>
    /// Checks the chosen role and stores against the pickers. An inactive role or store is only accepted
    /// when the employee already has it (so unrelated edits are never blocked).
    /// </summary>
    private static void ValidateAssignments(
        OutputUserOptionList options,
        int fkRole,
        List<int> storeIds,
        int? currentRoleId,
        List<int> currentStoreIds,
        Dictionary<string, string[]> errors)
    {
        var role = options.Roles.FirstOrDefault(r => r.IdRole == fkRole);
        if (role is null)
        {
            AddError(errors, "FkRole", "The selected role does not exist.");
        }
        else if (!role.IsActive && role.IdRole != currentRoleId)
        {
            AddError(errors, "FkRole", "The selected role is inactive. Choose an active role.");
        }

        if (storeIds.Count == 0)
        {
            AddError(errors, "StoreIds", "Select at least one store. Employees need a store to sign in.");
            return;
        }

        foreach (var storeId in storeIds)
        {
            var store = options.Stores.FirstOrDefault(s => s.IdStore == storeId);
            if (store is null || (!store.IsActive && !currentStoreIds.Contains(storeId)))
            {
                AddError(errors, "StoreIds", "One or more selected stores are not available.");
                return;
            }
        }
    }

    /// <summary>The default store must be one of the selected stores; falls back to the first one.</summary>
    private static int? ResolveDefaultStore(int? requested, List<int> storeIds)
    {
        if (storeIds.Count == 0) return null;
        return requested.HasValue && storeIds.Contains(requested.Value) ? requested.Value : storeIds[0];
    }

    /// <summary>Applies the shared StrongPassword rule (8 to 64 characters, upper, lower, digit, special).</summary>
    private static string? PasswordPolicyError(string password, string displayName)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return $"The {displayName} field is required.";
        }

        var context = new DataAnnotations.ValidationContext(new object()) { DisplayName = displayName };
        var result = new StrongPasswordAttribute(8, 64).GetValidationResult(password, context);
        return result?.ErrorMessage;
    }

    /// <summary>Random, policy-compliant temporary password (no look-alike characters).</summary>
    private static string GenerateTemporaryPassword()
    {
        const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lower = "abcdefghijkmnopqrstuvwxyz";
        const string digits = "23456789";
        const string special = "!@#$%*?-_";
        const string all = upper + lower + digits + special;

        var chars = new List<char>(TemporaryPasswordLength)
        {
            upper[RandomNumberGenerator.GetInt32(upper.Length)],
            lower[RandomNumberGenerator.GetInt32(lower.Length)],
            digits[RandomNumberGenerator.GetInt32(digits.Length)],
            special[RandomNumberGenerator.GetInt32(special.Length)]
        };

        while (chars.Count < TemporaryPasswordLength)
        {
            chars.Add(all[RandomNumberGenerator.GetInt32(all.Length)]);
        }

        for (var i = chars.Count - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        return new string(chars.ToArray());
    }

    private static void AddError(Dictionary<string, string[]> errors, string field, string message)
    {
        errors.TryAdd(field, new[] { message });
    }

    private static void ThrowIfInvalid(Dictionary<string, string[]> errors)
    {
        if (errors.Count == 0) return;

        var developerMessage = string.Join(" ", errors.SelectMany(e => e.Value));
        throw new ValidationException(FieldSummary, developerMessage, errors);
    }

    private Task AuditAsync(int actorUserId, int idUser, string action, string details, CancellationToken ct) =>
        _audit.WriteLogAsync(new InputAuditLogCreate
        {
            ActorUserId = actorUserId,
            EntityName = "User",
            EntityId = idUser,
            Action = action,
            Details = details
        }, ct);
}
