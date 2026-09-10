namespace Ecommerce.Helpers.Common
{
    public interface IMediaStorageProvider
    {
        string Name { get; }

        Task<MediaStoreResult> UploadAsync(
            IFormFile file,
            MediaUploadContext context,
            CancellationToken cancellationToken = default);

        Task DeleteAsync(
            string? keyOrUrl,
            MediaDeleteContext? context = null,
            CancellationToken cancellationToken = default);
    }
}
