namespace Ecommerce.Helpers.Common
{
    public class MediaUploadContext
    {
        /// <summary>Logical destination such as products, categories, brands, sku, or employees.</summary>
        public string Destination { get; set; } = string.Empty;

        /// <summary>Physical folder under wwwroot for local storage.</summary>
        public string? LocalFolderPath { get; set; }

        /// <summary>Public path prefix such as uploads/products/{slug}/media. Used by the local provider.</summary>
        public string? RelativeUrlDirectory { get; set; }

        /// <summary>When true, existing files in the local destination folder are removed first.</summary>
        public bool ReplaceExistingInFolder { get; set; }
    }

    public class MediaDeleteContext
    {
        public string? WebRootPath { get; set; }
    }

    public class MediaStoreResult
    {
        public string FileName { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;

        public string Key { get; set; } = string.Empty;
    }
}
