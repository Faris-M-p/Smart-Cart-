namespace SmartCart.Application.Abstractions.Security;

public interface ILicenseService
{
    Task AssertCanWriteAsync(Guid companyAccountId, CancellationToken cancellationToken = default);
    Task AssertModuleAsync(Guid companyAccountId, string moduleCode, CancellationToken cancellationToken = default);
    Task AssertStoreCountAsync(Guid companyAccountId, int requestedStoreCount, CancellationToken cancellationToken = default);
    Task<LicenseSnapshot> GetSnapshotAsync(Guid companyAccountId, CancellationToken cancellationToken = default);
}

public sealed class LicenseSnapshot
{
    public required Guid CompanyAccountId { get; init; }
    public required string Status { get; init; }
    public required bool CanWrite { get; init; }
    public required int MaxStoreCount { get; init; }
    public required string StoreMode { get; init; }
    public DateTimeOffset? ExpiresOn { get; init; }
    public IReadOnlyList<string> EnabledModules { get; init; } = Array.Empty<string>();
}

public sealed class LicenseViolationException : InvalidOperationException
{
    public LicenseViolationException(string message) : base(message) { }
}
