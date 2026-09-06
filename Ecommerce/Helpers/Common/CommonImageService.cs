using Ecommerce.Models.Entities;
using static Ecommerce.Models.Admin.ProductVariantModel;

namespace Ecommerce.Helpers.Common
{
    public class CommonImageService
    {
        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".webp"
        };

        private const long MaxFileBytes = 2 * 1024 * 1024;

        // Used by controller before upload processing to validate common image request rules.
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
                var fileError = ValidateImageFile(file);
                if (!string.IsNullOrWhiteSpace(fileError))
                {
                    return fileError;
                }
            }

            return null;
        }

        // Used by ValidateUploadInput to enforce extension and size limits per file.
        private string? ValidateImageFile(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName);
            if (!AllowedExtensions.Contains(ext))
            {
                return "Only jpg, jpeg, png, and webp files are allowed.";
            }

            if (file.Length <= 0 || file.Length > MaxFileBytes)
            {
                return "Each image must be less than or equal to 2 MB.";
            }

            return null;
        }

        // Used by controller to apply client order safely while preserving missing rows.
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

        // Used by controller to keep API response mapping in one place.
        public ProductVariantImageDto MapToDto(ProductVariantImageEntity row) =>
            new()
            {
                ID_ProductVariantImage = row.ID_ProductVariantImage,
                FK_ProductVariant = row.FK_ProductVariant,
                ImageUrl = row.ImageUrl,
                IsPrimary = row.IsPrimary,
                DisplayOrder = row.DisplayOrder
            };

        // Used by controller to resolve the physical upload directory.
        public string GetUploadRoot(string webRootPath, string productSlug, string sku) =>
            Path.Combine(webRootPath, "uploads", "products", NormalizePathSegment(productSlug), NormalizePathSegment(sku));

        // Used by controller to generate URL stored in database.
        public string BuildImageUrl(string productSlug, string sku, string fileName) =>
            $"/uploads/products/{NormalizePathSegment(productSlug)}/{NormalizePathSegment(sku)}/{fileName}";

        public int GetNextImageNumber(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                return 1;
            }

            var maxNumber = 0;
            foreach (var file in Directory.GetFiles(folderPath))
            {
                var name = Path.GetFileNameWithoutExtension(file);
                if (int.TryParse(name, out var n) && n > maxNumber)
                {
                    maxNumber = n;
                }
            }

            return maxNumber + 1;
        }

        // Used by controller when DB write fails and files must be cleaned up.
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
