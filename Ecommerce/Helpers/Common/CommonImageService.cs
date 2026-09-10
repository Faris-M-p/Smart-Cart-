using Ecommerce.Models.Entities;
using static Ecommerce.Models.Admin.ProductModel;
using static Ecommerce.Models.Admin.ProductVariantModel;

namespace Ecommerce.Helpers.Common
{
    public class CommonImageService
    {
        private readonly IMediaStorageProvider _storage;

        public CommonImageService(IMediaStorageProvider storage)
        {
            _storage = storage;
        }

        private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".webp"
        };

        private static readonly HashSet<string> AllowedVideoExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".mp4", ".webm"
        };

        private const long MaxImageBytes = 2 * 1024 * 1024;
        private const long MaxVideoBytes = 20 * 1024 * 1024;
        private const int MaxProductMedia = 5;
        private const int MaxProductVideos = 1;

        public string? ValidateImageFile(IFormFile file)
        {
            if (file == null || file.Length <= 0)
            {
                return "Please select a valid image.";
            }

            var ext = Path.GetExtension(file.FileName);
            if (!AllowedImageExtensions.Contains(ext))
            {
                return "Only jpg, jpeg, png, and webp files are allowed.";
            }

            if (file.Length > MaxImageBytes)
            {
                return "Each image must be less than or equal to 2 MB.";
            }

            return null;
        }

        public string? ValidateVideoFile(IFormFile file)
        {
            if (file == null || file.Length <= 0)
            {
                return "Please select a valid video.";
            }

            var ext = Path.GetExtension(file.FileName);
            if (!AllowedVideoExtensions.Contains(ext))
            {
                return "Only mp4 and webm videos are allowed.";
            }

            if (file.Length > MaxVideoBytes)
            {
                return "Each video must be less than or equal to 20 MB.";
            }

            return null;
        }

        public string? ValidateMediaFile(IFormFile file, bool allowVideo)
        {
            var mediaType = ResolveMediaType(file.FileName);
            if (mediaType == null)
            {
                return allowVideo
                    ? "Only jpg, jpeg, png, webp images and mp4/webm videos are allowed."
                    : "Only jpg, jpeg, png, and webp files are allowed.";
            }

            if (mediaType == "Video" && !allowVideo)
            {
                return "Video is not allowed.";
            }

            return mediaType == "Video"
                ? ValidateVideoFile(file)
                : ValidateImageFile(file);
        }

        public string? ValidateUploadInput(ProductVariantImageUploadInput input)
        {
            if (input == null || input.SKUId <= 0)
            {
                return "Invalid SKU.";
            }

            if (input.Files == null || input.Files.Count == 0)
            {
                return "Please select at least one image.";
            }

            foreach (var file in input.Files)
            {
                var fileError = ValidateMediaFile(file, allowVideo: false);
                if (!string.IsNullOrWhiteSpace(fileError))
                {
                    return fileError;
                }
            }

            return null;
        }

        public string? ValidateProductMediaLimits(int videoCount, int totalCount)
        {
            if (totalCount > MaxProductMedia)
            {
                return $"Maximum {MaxProductMedia} media items are allowed per product.";
            }

            if (videoCount > MaxProductVideos)
            {
                return "Only one video is allowed per product.";
            }

            return null;
        }

        public string? ResolveMediaType(string fileName)
        {
            var ext = Path.GetExtension(fileName);
            if (AllowedImageExtensions.Contains(ext))
            {
                return "Image";
            }

            if (AllowedVideoExtensions.Contains(ext))
            {
                return "Video";
            }

            return null;
        }

        public List<ProductVariantImageEntity> ReorderExisting(
            List<ProductVariantImageEntity> existing,
            List<int>? orderedImageIds)
        {
            if (orderedImageIds == null || orderedImageIds.Count == 0)
            {
                return existing
                    .OrderBy(x => x.DisplayOrder)
                    .ThenBy(x => x.ID_ProductVariantImage)
                    .ToList();
            }

            var byId = existing.ToDictionary(x => x.ID_ProductVariantImage, x => x);
            var result = new List<ProductVariantImageEntity>();

            foreach (var id in orderedImageIds.Distinct())
            {
                if (byId.TryGetValue(id, out var row))
                {
                    result.Add(row);
                    byId.Remove(id);
                }
            }

            foreach (var remaining in byId.Values.OrderBy(x => x.DisplayOrder).ThenBy(x => x.ID_ProductVariantImage))
            {
                result.Add(remaining);
            }

            return result;
        }

        public ProductVariantImageDto MapToDto(ProductVariantImageEntity row) =>
            new()
            {
                ID_ProductVariantImage = row.ID_ProductVariantImage,
                FK_ProductVariant = row.FK_ProductVariant,
                ImageUrl = row.ImageUrl,
                IsPrimary = row.IsPrimary,
                DisplayOrder = row.DisplayOrder
            };

        public ProductMediaDto MapToDto(ProductMediaEntity row) =>
            new()
            {
                ID_ProductMedia = row.ID_ProductMedia,
                FK_Product = row.FK_Product,
                MediaType = row.MediaType,
                MediaUrl = row.MediaUrl,
                IsPrimary = row.IsPrimary,
                DisplayOrder = row.DisplayOrder
            };

        public string GetUploadRoot(string webRootPath, string productSlug, string sku) =>
            Path.Combine(webRootPath, "uploads", "products", NormalizePathSegment(productSlug), NormalizePathSegment(sku));

        public string BuildImageUrl(string productSlug, string sku, string fileName) =>
            $"/uploads/products/{NormalizePathSegment(productSlug)}/{NormalizePathSegment(sku)}/{fileName}";

        public string GetProductMediaUploadRoot(string webRootPath, string productSlug) =>
            Path.Combine(webRootPath, "uploads", "products", NormalizePathSegment(productSlug), "media");

        public string BuildProductMediaUrl(string productSlug, string fileName) =>
            $"/uploads/products/{NormalizePathSegment(productSlug)}/media/{fileName}";

        public string GetEmployeeUploadRoot(string webRootPath, int employeeId) =>
            Path.Combine(webRootPath, "uploads", "employees", employeeId.ToString());

        public string BuildEmployeeImageUrl(int employeeId, string fileName) =>
            $"/uploads/employees/{employeeId}/{fileName}";

        public string GetCategoryUploadRoot(string webRootPath, int categoryId) =>
            Path.Combine(webRootPath, "uploads", "categories", categoryId.ToString());

        public string BuildCategoryImageUrl(int categoryId, string fileName) =>
            $"/uploads/categories/{categoryId}/{fileName}";

        public string GetSubCategoryUploadRoot(string webRootPath, int subCategoryId) =>
            Path.Combine(webRootPath, "uploads", "subcategories", subCategoryId.ToString());

        public string BuildSubCategoryImageUrl(int subCategoryId, string fileName) =>
            $"/uploads/subcategories/{subCategoryId}/{fileName}";

        public string GetBrandUploadRoot(string webRootPath, int brandId) =>
            Path.Combine(webRootPath, "uploads", "brands", brandId.ToString());

        public string BuildBrandImageUrl(int brandId, string fileName) =>
            $"/uploads/brands/{brandId}/{fileName}";

        public string GetEntityUploadRoot(string webRootPath, string folder, int entityId) =>
            Path.Combine(webRootPath, "uploads", NormalizePathSegment(folder), entityId.ToString());

        public string BuildEntityImageUrl(string folder, int entityId, string fileName) =>
            $"/uploads/{NormalizePathSegment(folder)}/{entityId}/{fileName}";

        /// <summary>
        /// Common upload entry point. Destination is a logical folder such as products or categories.
        /// The active storage provider decides where the file is stored.
        /// </summary>
        public async Task<string> UploadAsync(IFormFile file, string destination)
        {
            var uploaded = await _storage.UploadAsync(file, new MediaUploadContext
            {
                Destination = destination
            });
            return uploaded.Url;
        }

        public async Task<string> SaveFileAsync(IFormFile file, string folderPath)
        {
            var uploaded = await _storage.UploadAsync(file, new MediaUploadContext
            {
                Destination = ResolveDestination(folderPath),
                LocalFolderPath = folderPath
            });
            return uploaded.FileName;
        }

        public async Task<string> SaveEmployeeProfileAsync(IFormFile file, string folderPath)
        {
            return await SaveFileAsync(file, folderPath);
        }

        public async Task<string?> SaveSingleImageAsync(
            IFormFile? file,
            string webRootPath,
            string folder,
            int entityId,
            string? previousUrl = null)
        {
            var destination = NormalizePathSegment(folder);
            var folderPath = GetEntityUploadRoot(webRootPath, destination, entityId);

            if (file == null)
            {
                await TryDeleteStoredAsync(previousUrl, webRootPath);
                ClearFolder(folderPath);
                return null;
            }

            var uploaded = await _storage.UploadAsync(file, new MediaUploadContext
            {
                Destination = destination,
                LocalFolderPath = folderPath,
                RelativeUrlDirectory = $"uploads/{destination}/{entityId}",
                ReplaceExistingInFolder = true
            });

            return uploaded.Url;
        }

        public string ToAbsolutePath(string webRootPath, string publicUrl)
        {
            if (string.IsNullOrWhiteSpace(webRootPath) || string.IsNullOrWhiteSpace(publicUrl) || publicUrl.Contains("..", StringComparison.Ordinal))
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

        public void TryDeleteByUrl(string webRootPath, string? publicUrl)
        {
            TryDeleteStoredAsync(publicUrl, webRootPath).GetAwaiter().GetResult();
        }

        public async Task TryDeleteStoredAsync(string? publicUrl, string? webRootPath = null)
        {
            if (string.IsNullOrWhiteSpace(publicUrl))
            {
                return;
            }

            try
            {
                await _storage.DeleteAsync(publicUrl, new MediaDeleteContext
                {
                    WebRootPath = webRootPath
                });
            }
            catch
            {
                // Best-effort storage cleanup only.
            }
        }

        public void TryDeleteFile(string absolutePath)
        {
            try
            {
                if (File.Exists(absolutePath))
                {
                    File.Delete(absolutePath);
                }
            }
            catch
            {
                // Best-effort file cleanup only.
            }
        }

        private void ClearFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                return;
            }

            foreach (var existing in Directory.GetFiles(folderPath))
            {
                TryDeleteFile(existing);
            }
        }

        private static readonly string[] KnownDestinations =
        {
            "products", "categories", "subcategories", "brands", "banners", "sku", "employees"
        };

        private static string ResolveDestination(string folderPath)
        {
            var normalized = (folderPath ?? string.Empty).Replace('\\', '/').ToLowerInvariant();
            foreach (var folder in KnownDestinations)
            {
                if (normalized == folder || normalized.Contains($"/{folder}/") || normalized.EndsWith($"/{folder}"))
                {
                    return folder;
                }
            }

            return "products";
        }

        private static string NormalizePathSegment(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "unknown";
            }

            var normalized = value.Trim();
            foreach (var invalid in Path.GetInvalidFileNameChars())
            {
                normalized = normalized.Replace(invalid, '-');
            }

            normalized = normalized.Replace('/', '-').Replace('\\', '-');
            return string.IsNullOrWhiteSpace(normalized) ? "unknown" : normalized;
        }
    }
}
