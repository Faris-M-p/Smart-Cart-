using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ecommerce.DataAccess;
using Ecommerce.Filters;
using Ecommerce.Helpers.Common;
using Ecommerce.Interface.Admin;
using Ecommerce.Models.Entities;
using Ecommerce.Models.Enums;
using static Ecommerce.Models.Admin.ProductModel;
using static Ecommerce.Models.Admin.CategoryModel;
using static Ecommerce.Models.Admin.SubCategoryModel;
using static Ecommerce.Models.Admin.BrandModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Controllers.Admin
{
    [Route("Admin/Product")]
    public class ProductController : Controller
    {
        private readonly IProductInterface _productInterface;
        private readonly ICategoryInterface _categoryInterface;
        private readonly ISubCategoryInterface _subCategoryInterface;
        private readonly IBrandInterface _brandInterface;
        private readonly EcommerceDbContext _dbContext;
        private readonly CommonImageService _commonImageService;
        private readonly IWebHostEnvironment _environment;

        public ProductController(
            IProductInterface productInterface,
            ICategoryInterface categoryInterface,
            ISubCategoryInterface subCategoryInterface,
            IBrandInterface brandInterface,
            EcommerceDbContext dbContext,
            CommonImageService commonImageService,
            IWebHostEnvironment environment)
        {
            _productInterface = productInterface;
            _categoryInterface = categoryInterface;
            _subCategoryInterface = subCategoryInterface;
            _brandInterface = brandInterface;
            _dbContext = dbContext;
            _commonImageService = commonImageService;
            _environment = environment;
        }

        [Route("")]
        [Route("Index")]
        [RequirePermission("Products.View")]
        public IActionResult Index()
        {
            return View("~/Views/Admin/Product/Index.cshtml");
        }

        [HttpPost]
        [Route("GetProductList")]
        [RequirePermission("Products.View")]
        public async Task<IActionResult> GetProductList([FromBody] ProductListInputVIEW viewInput)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new ApiResponse<TableOutput<Product>>
                    {
                        Success = false,
                        Message = "Validation failed",
                        Errors = errors
                    });
                }

                var input = new ProductListInput
                {
                    SearchText = viewInput.SearchText,
                    FilterCategoryIDs = viewInput.FilterCategoryIDs,
                    FilterSubCategoryIDs = viewInput.FilterSubCategoryIDs,
                    FilterBrandIDs = viewInput.FilterBrandIDs,
                    PageIndex = viewInput.PageIndex,
                    PageSize = viewInput.PageSize,
                    SortColumn = viewInput.SortColumn,
                    SortMode = viewInput.SortMode
                };

                var result = await _productInterface.GetProductListAsync(input);

                return Ok(new ApiResponse<TableOutput<Product>>
                {
                    Success = true,
                    Message = "Products loaded successfully",
                    Data = result
                });
            }
            catch
            {
                throw;
            }
        }

        [HttpPost]
        [Route("Create")]
        [RequestSizeLimit(50 * 1024 * 1024)]
        [RequirePermission("Products.Create")]
        public async Task<IActionResult> Create([FromForm] ProductUpdateInputVIEW viewInput)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new { message = "Validation failed.", errors });
                }

                var sub = await _subCategoryInterface.GetSubCategoryByIdAsync(viewInput.FK_SubCategory);
                if (sub == null || sub.Cancelled)
                {
                    return BadRequest(new { message = "Validation failed.", errors = new[] { "Invalid subcategory." } });
                }

                if (sub.FK_Category != viewInput.FK_Category)
                {
                    return BadRequest(new { message = "Validation failed.", errors = new[] { "Subcategory does not belong to the selected category." } });
                }

                var input = new ProductUpdateInput
                {
                    ID_Product = 0,
                    Name = viewInput.Name,
                    Slug = viewInput.Slug,
                    Description = viewInput.Description,
                    FK_SubCategory = viewInput.FK_SubCategory,
                    FK_Brand = viewInput.FK_Brand,
                    IsActive = viewInput.IsActive,
                    SellOnline = viewInput.SellOnline,
                    EnterBy = 1
                };

                var result = await _productInterface.CreateProductAsync(input);
                if (result.StatusCode)
                {
                    var mediaError = await SyncProductMediaAsync(viewInput, (int)result.ResponseCode);
                    if (!string.IsNullOrWhiteSpace(mediaError))
                    {
                        return BadRequest(new { message = mediaError });
                    }
                }

                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost]
        [Route("Update")]
        [RequestSizeLimit(50 * 1024 * 1024)]
        [RequirePermission("Products.Edit")]
        public async Task<IActionResult> Update([FromForm] ProductUpdateInputVIEW viewInput)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new { message = "Validation failed.", errors });
                }

                var sub = await _subCategoryInterface.GetSubCategoryByIdAsync(viewInput.FK_SubCategory);
                if (sub == null || sub.Cancelled)
                {
                    return BadRequest(new { message = "Validation failed.", errors = new[] { "Invalid subcategory." } });
                }

                if (sub.FK_Category != viewInput.FK_Category)
                {
                    return BadRequest(new { message = "Validation failed.", errors = new[] { "Subcategory does not belong to the selected category." } });
                }

                var input = new ProductUpdateInput
                {
                    ID_Product = viewInput.ID_Product,
                    Name = viewInput.Name,
                    Slug = viewInput.Slug,
                    Description = viewInput.Description,
                    FK_SubCategory = viewInput.FK_SubCategory,
                    FK_Brand = viewInput.FK_Brand,
                    IsActive = viewInput.IsActive,
                    SellOnline = viewInput.SellOnline,
                    EnterBy = 1
                };

                var result = await _productInterface.UpdateProductAsync(input);
                if (result.StatusCode)
                {
                    var mediaError = await SyncProductMediaAsync(viewInput, viewInput.ID_Product);
                    if (!string.IsNullOrWhiteSpace(mediaError))
                    {
                        return BadRequest(new { message = mediaError });
                    }
                }

                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost]
        [Route("Delete")]
        [RequirePermission("Products.Delete")]
        public async Task<IActionResult> Delete([FromBody] ProductDeleteInputVIEW viewInput)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new { message = "Validation failed.", errors });
                }

                var input = new ProductDeleteInput
                {
                    ID_Product = viewInput.ID_Product,
                    EnterBy = 1
                };

                var result = await _productInterface.DeleteProductAsync(input);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        [HttpGet]
        [Route("GetById/{id}")]
        [RequirePermission("Products.View")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var product = await _productInterface.GetProductByIdAsync(id);

                if (product != null)
                {
                    return Ok(product);
                }

                return NotFound(new { message = "Product not found." });
            }
            catch
            {
                throw;
            }
        }

        [HttpGet]
        [Route("GetMedia/{productId}")]
        [RequirePermission("Products.View")]
        public async Task<IActionResult> GetMedia(int productId)
        {
            if (productId <= 0)
            {
                return BadRequest(new { message = "Invalid product ID." });
            }

            var rows = await GetProductMediaAsync(productId);
            return Ok(rows.Select(_commonImageService.MapToDto).ToList());
        }

        [HttpPost]
        [Route("DeleteMedia")]
        [RequirePermission("Products.Edit")]
        public async Task<IActionResult> DeleteMedia([FromBody] ProductMediaDeleteInput input)
        {
            if (input == null || input.MediaId <= 0)
            {
                return BadRequest(new { message = "Invalid media." });
            }

            var media = await _dbContext.ProductMedia.FirstOrDefaultAsync(x => x.ID_ProductMedia == input.MediaId);
            if (media == null)
            {
                return BadRequest(new { message = "Media not found." });
            }

            var remaining = await _dbContext.ProductMedia
                .Where(x => x.FK_Product == media.FK_Product && x.ID_ProductMedia != media.ID_ProductMedia)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.ID_ProductMedia)
                .ToListAsync();

            _dbContext.ProductMedia.Remove(media);
            for (var i = 0; i < remaining.Count; i++)
            {
                remaining[i].DisplayOrder = i;
                remaining[i].UpdatedAt = DateTime.Now;
            }

            if (media.IsPrimary && remaining.Count > 0 && !remaining.Any(x => x.IsPrimary))
            {
                remaining[0].IsPrimary = true;
            }

            await _dbContext.SaveChangesAsync();
            await _commonImageService.TryDeleteStoredAsync(media.MediaUrl, _environment.WebRootPath);
            return Ok(new CommonResponse
            {
                ResponseCode = media.ID_ProductMedia,
                StatusCode = true,
                ResponseMsg = "Media deleted successfully."
            });
        }

        [HttpPost]
        [Route("SetPrimaryMedia")]
        [RequirePermission("Products.Edit")]
        public async Task<IActionResult> SetPrimaryMedia([FromBody] ProductMediaSetPrimaryInput input)
        {
            if (input == null || input.ProductId <= 0 || input.MediaId <= 0)
            {
                return BadRequest(new { message = "Invalid request." });
            }

            var media = await GetProductMediaAsync(input.ProductId);
            if (media.Count == 0)
            {
                return BadRequest(new { message = "No media found for this product." });
            }

            if (!media.Any(x => x.ID_ProductMedia == input.MediaId))
            {
                return BadRequest(new { message = "Primary media does not belong to this product." });
            }

            var ordered = ReorderProductMedia(media, input.OrderedMediaIds);
            for (var i = 0; i < ordered.Count; i++)
            {
                ordered[i].DisplayOrder = i;
                ordered[i].IsPrimary = ordered[i].ID_ProductMedia == input.MediaId;
                ordered[i].UpdatedAt = DateTime.Now;
            }

            await _dbContext.SaveChangesAsync();
            var rows = await GetProductMediaAsync(input.ProductId);
            return Ok(new
            {
                ResponseCode = input.MediaId,
                StatusCode = true,
                ResponseMsg = "Primary media updated successfully.",
                Media = rows.Select(_commonImageService.MapToDto).ToList()
            });
        }

        [HttpGet]
        [Route("GetCategories")]
        [RequirePermission("Products.View")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var input = new CategoryListInput
                {
                    PageIndex = 1,
                    PageSize = 1000,
                    SearchText = string.Empty,
                    FilterCategoryIDs = string.Empty,
                    SortColumn = 0,
                    SortMode = "ASC"
                };

                var result = await _categoryInterface.GetCategoryListAsync(input);
                return Ok(result?.TableData ?? new List<Category>());
            }
            catch
            {
                throw;
            }
        }

        [HttpGet]
        [Route("GetSubCategories")]
        [RequirePermission("Products.View")]
        public async Task<IActionResult> GetSubCategories(int? categoryId = null)
        {
            try
            {
                var input = new SubCategoryListInput
                {
                    PageIndex = 1,
                    PageSize = 1000,
                    SearchText = string.Empty,
                    FilterCategoryIDs = categoryId.HasValue ? $"[{{\"ID_Value\":{categoryId}}}]" : string.Empty,
                    FilterSubCategoryIDs = string.Empty,
                    SortColumn = (int)SubCategorySortColumn.Name,
                    SortMode = "ASC"
                };

                var result = await _subCategoryInterface.GetSubCategoryListAsync(input);
                return Ok(result?.TableData ?? new List<SubCategory>());
            }
            catch
            {
                throw;
            }
        }

        [HttpGet]
        [Route("GetBrands")]
        [RequirePermission("Products.View")]
        public async Task<IActionResult> GetBrands()
        {
            try
            {
                var input = new BrandListInput
                {
                    PageIndex = 1,
                    PageSize = 2000,
                    SearchText = string.Empty,
                    FilterBrandIDs = string.Empty,
                    SortColumn = 0,
                    SortMode = "ASC"
                };

                var result = await _brandInterface.GetBrandListAsync(input);
                var data = result?.TableData;
                if (data == null)
                {
                    return Ok(Array.Empty<object>());
                }

                var rows = data
                    .Where(b => !b.Cancelled && b.IsActive)
                    .Select(b => new { ID_Brand = b.BrandID, Name = b.BrandName, ImageUrl = b.ImageUrl })
                    .ToList();
                return Ok(rows);
            }
            catch
            {
                throw;
            }
        }

        private Task<List<ProductMediaEntity>> GetProductMediaAsync(int productId) =>
            _dbContext.ProductMedia
                .Where(x => x.FK_Product == productId)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.ID_ProductMedia)
                .ToListAsync();

        private static List<ProductMediaEntity> ReorderProductMedia(List<ProductMediaEntity> existing, List<int>? orderedIds)
        {
            if (orderedIds == null || orderedIds.Count == 0)
            {
                return existing;
            }

            var byId = existing.ToDictionary(x => x.ID_ProductMedia, x => x);
            var result = new List<ProductMediaEntity>();
            foreach (var id in orderedIds.Distinct())
            {
                if (byId.TryGetValue(id, out var row))
                {
                    result.Add(row);
                    byId.Remove(id);
                }
            }

            result.AddRange(byId.Values);
            return result;
        }

        private async Task<string?> SyncProductMediaAsync(ProductUpdateInputVIEW viewInput, int productId)
        {
            var product = await _productInterface.GetProductByIdAsync(productId);
            if (product == null || string.IsNullOrWhiteSpace(product.Slug))
            {
                return "Product slug is required before media can be saved.";
            }

            var existing = await GetProductMediaAsync(productId);
            var removedIds = (viewInput.RemovedMediaIds ?? new List<int>()).Where(x => x > 0).Distinct().ToHashSet();
            var orderedExisting = ReorderProductMedia(existing, viewInput.ExistingMediaOrder)
                .Where(x => !removedIds.Contains(x.ID_ProductMedia))
                .ToList();

            foreach (var row in existing.Where(x => removedIds.Contains(x.ID_ProductMedia)))
            {
                _dbContext.ProductMedia.Remove(row);
                await _commonImageService.TryDeleteStoredAsync(row.MediaUrl, _environment.WebRootPath);
            }

            var incomingFiles = viewInput.Files ?? new List<IFormFile>();
            var newRows = new List<ProductMediaEntity>();

            foreach (var file in incomingFiles)
            {
                var fileError = _commonImageService.ValidateMediaFile(file, allowVideo: true);
                if (!string.IsNullOrWhiteSpace(fileError))
                {
                    return fileError;
                }

                var mediaType = _commonImageService.ResolveMediaType(file.FileName);
                if (string.IsNullOrWhiteSpace(mediaType))
                {
                    return "Invalid media file.";
                }

                var uploadRoot = _commonImageService.GetProductMediaUploadRoot(_environment.WebRootPath, product.Slug);
                var fileName = await _commonImageService.SaveFileAsync(file, uploadRoot);
                var mediaUrl = _commonImageService.BuildProductMediaUrl(product.Slug, fileName);
                newRows.Add(new ProductMediaEntity
                {
                    FK_Product = productId,
                    MediaType = mediaType,
                    MediaUrl = mediaUrl,
                    IsPrimary = false,
                    DisplayOrder = 0,
                    CreatedAt = DateTime.Now
                });
            }

            var finalRows = orderedExisting.Concat(newRows).ToList();
            var videoCount = finalRows.Count(x => string.Equals(x.MediaType, "Video", StringComparison.OrdinalIgnoreCase));
            var limitError = _commonImageService.ValidateProductMediaLimits(videoCount, finalRows.Count);
            if (!string.IsNullOrWhiteSpace(limitError))
            {
                return limitError;
            }

            foreach (var row in finalRows)
            {
                row.IsPrimary = false;
            }

            if (viewInput.PrimaryMediaId.HasValue && viewInput.PrimaryMediaId.Value > 0)
            {
                var target = finalRows.FirstOrDefault(x => x.ID_ProductMedia == viewInput.PrimaryMediaId.Value);
                if (target != null)
                {
                    target.IsPrimary = true;
                }
            }
            else if (viewInput.PrimaryIndex.HasValue &&
                     viewInput.PrimaryIndex.Value >= 0 &&
                     viewInput.PrimaryIndex.Value < newRows.Count)
            {
                newRows[viewInput.PrimaryIndex.Value].IsPrimary = true;
            }
            else
            {
                var originalPrimary = orderedExisting.FirstOrDefault(x => x.IsPrimary);
                if (originalPrimary != null)
                {
                    originalPrimary.IsPrimary = true;
                }
            }

            if (finalRows.Count > 0 && !finalRows.Any(x => x.IsPrimary))
            {
                finalRows[0].IsPrimary = true;
            }

            for (var i = 0; i < finalRows.Count; i++)
            {
                finalRows[i].DisplayOrder = i;
                finalRows[i].UpdatedAt = DateTime.Now;
            }

            if (newRows.Count > 0)
            {
                await _dbContext.ProductMedia.AddRangeAsync(newRows);
            }

            await _dbContext.SaveChangesAsync();
            return null;
        }
    }
}
