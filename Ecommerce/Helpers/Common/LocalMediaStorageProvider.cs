namespace Ecommerce.Helpers.Common
{
    public class LocalMediaStorageProvider : IMediaStorageProvider
    {
        private readonly IWebHostEnvironment _environment;

        public LocalMediaStorageProvider(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public string Name => "Local";

        public async Task<MediaStoreResult> UploadAsync(
            IFormFile file,
            MediaUploadContext context,
            CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length <= 0)
            {
                throw new ArgumentException("A file is required.", nameof(file));
            }

            var folderPath = ResolveLocalFolder(context);
            Directory.CreateDirectory(folderPath);

            if (context.ReplaceExistingInFolder)
            {
                ClearFolder(folderPath);
            }

            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = ".bin";
            }

            var fileName = CreateUniqueFileName(folderPath, extension);
            var fullPath = Path.Combine(folderPath, fileName);

            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            var url = BuildPublicUrl(context, fileName);
            return new MediaStoreResult
            {
                FileName = fileName,
                Url = url,
                Key = url
            };
        }

        public Task DeleteAsync(
            string? keyOrUrl,
            MediaDeleteContext? context = null,
            CancellationToken cancellationToken = default)
        {
            var webRoot = context?.WebRootPath ?? _environment.WebRootPath;
            if (string.IsNullOrWhiteSpace(keyOrUrl) || string.IsNullOrWhiteSpace(webRoot))
            {
                return Task.CompletedTask;
            }

            var absolutePath = ToAbsoluteUploadsPath(webRoot, keyOrUrl);
            if (string.IsNullOrWhiteSpace(absolutePath))
            {
                return Task.CompletedTask;
            }

            try
            {
                if (File.Exists(absolutePath))
                {
                    File.Delete(absolutePath);
                }
            }
            catch
            {
                // Best-effort local cleanup only.
            }

            return Task.CompletedTask;
        }

        private string ResolveLocalFolder(MediaUploadContext context)
        {
            if (!string.IsNullOrWhiteSpace(context.LocalFolderPath))
            {
                return context.LocalFolderPath;
            }

            var destination = NormalizeDestination(context.Destination);
            return Path.Combine(_environment.WebRootPath, "uploads", destination);
        }

        private static string BuildPublicUrl(MediaUploadContext context, string fileName)
        {
            if (!string.IsNullOrWhiteSpace(context.RelativeUrlDirectory))
            {
                var relative = context.RelativeUrlDirectory.Replace('\\', '/').Trim('/');
                return $"/{relative}/{fileName}";
            }

            if (!string.IsNullOrWhiteSpace(context.LocalFolderPath))
            {
                return fileName;
            }

            var destination = NormalizeDestination(context.Destination);
            return $"/uploads/{destination}/{fileName}";
        }

        private static string NormalizeDestination(string destination)
        {
            var value = (destination ?? string.Empty).Trim().Trim('/').ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(value)
                || value.Contains("..", StringComparison.Ordinal)
                || value.Contains('/')
                || value.Contains('\\'))
            {
                return "products";
            }

            return value;
        }

        private static string ToAbsoluteUploadsPath(string webRootPath, string publicUrl)
        {
            if (publicUrl.Contains("..", StringComparison.Ordinal))
            {
                return string.Empty;
            }

            var relative = publicUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var absolutePath = Path.GetFullPath(Path.Combine(webRootPath, relative));
            var uploadsRoot = Path.GetFullPath(Path.Combine(webRootPath, "uploads"));
            if (!absolutePath.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            return absolutePath;
        }

        private static void ClearFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                return;
            }

            foreach (var existing in Directory.GetFiles(folderPath))
            {
                try
                {
                    File.Delete(existing);
                }
                catch
                {
                    // Best-effort local cleanup only.
                }
            }
        }

        private static string CreateUniqueFileName(string folderPath, string extension)
        {
            string fileName;
            do
            {
                fileName = $"{Guid.NewGuid():N}{extension}";
            }
            while (File.Exists(Path.Combine(folderPath, fileName)));

            return fileName;
        }
    }
}
