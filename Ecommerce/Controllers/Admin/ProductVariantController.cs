using Ecommerce.DataAccess;
using Ecommerce.Interface.Admin;
using Ecommerce.Models.Entities;
using Ecommerce.Services.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using static Ecommerce.Models.Admin.ProductVariantModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Controllers.Admin
{
    [Route("Admin/ProductVariant")]
    public class ProductVariantController : Controller
    {
        private const int MaxFilesPerSku = 5;
        private readonly EcommerceDbContext _dbContext;
        private readonly IProductVariantInterface _productVariantInterface;
        private readonly IProductVariantImageRepository _productVariantImageRepository;
        private readonly ProductVariantImageService _productVariantImageService;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ProductVariantController> _logger;

        public ProductVariantController(
            EcommerceDbContext dbContext,
            IProductVariantInterface productVariantInterface,
            IProductVariantImageRepository productVariantImageRepository,
            ProductVariantImageService productVariantImageService,
            IWebHostEnvironment environment,
            ILogger<ProductVariantController> logger)
        {
            _dbContext = dbContext;
            _productVariantInterface = productVariantInterface;
            _productVariantImageRepository = productVariantImageRepository;
            _productVariantImageService = productVariantImageService;
            _environment = environment;
            _logger = logger;
        }

        [Route("")]
        [Route("Index")]
        public IActionResult Index()
        {
            return View("~/Views/Admin/ProductVariant/Index.cshtml");
        }

        [HttpPost]
        [Route("GetProductVariantList")]
        [Route("GetList")]
        public async Task<IActionResult> GetProductVariantList([FromBody] ProductVariantListInputVIEW viewInput)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new { message = "Validation failed.", errors });
            }

            var input = new ProductVariantListInput
            {
                FK_Product = viewInput.FK_Product,
                SearchText = viewInput.SearchText,
                FilterVariantIDs = viewInput.FilterVariantIDs,
                FilterVariantValueIDs = viewInput.FilterVariantValueIDs,
                PageIndex = viewInput.PageIndex,
                PageSize = viewInput.PageSize,
                SortColumn = viewInput.SortColumn,
                SortMode = viewInput.SortMode
            };

            var result = await _productVariantInterface.GetProductVariantListAsync(input);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _productVariantInterface.GetProductVariantByIdAsync(id);

            if (result != null)
            {
                return Ok(result);
            }

            return NotFound(new { message = "Product Variant not found." });
        }

        [HttpPost]
        [Route("Create")]
        [RequestSizeLimit(20 * 1024 * 1024)]
        public async Task<IActionResult> Create([FromForm] ProductVariantUpdateInputVIEW viewInput)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new { message = "Validation failed.", errors });
            }

            var variantRows = ResolveVariantValues(viewInput);
            if (variantRows.Count == 0)
            {
                return BadRequest(new { message = "At least one variant value row is required." });
            }

            var input = new ProductVariantUpdateInput
            {
                ID_ProductVariant = 0,
                FK_Product = viewInput.FK_Product,
                SKU = viewInput.SKU,
                VariantLabel = viewInput.VariantLabel,
                MRP = viewInput.MRP,
                SellingPrice = viewInput.SellingPrice,
                IsActive = viewInput.IsActive,
                IsDefault = viewInput.IsDefault,
                VariantValues = variantRows
                    .Where(v => v.VariantId > 0 && v.VariantValueId > 0)
                    .Select(v => new ProductVariantValueRowInput
                    {
                        VariantId = v.VariantId,
                        VariantValueId = v.VariantValueId
                    })
                    .ToList(),
                EnterBy = 1
            };

            await using var tx = await _dbContext.Database.BeginTransactionAsync();
            var savedFiles = new List<string>();
            var deferredDeleteFiles = new List<string>();
            try
            {
                var result = await _productVariantInterface.CreateProductVariantAsync(input);
                if (!result.StatusCode)
                {
                    await tx.RollbackAsync();
                    return BadRequest(result);
                }

                var skuId = (int)result.ResponseCode;
                if (skuId <= 0)
                {
                    await tx.RollbackAsync();
                    return BadRequest(Fail("Failed to save SKU."));
                }

                var imageSync = await SyncImagesInCreateUpdateAsync(viewInput, skuId);
                if (!imageSync.StatusCode)
                {
                    await tx.RollbackAsync();
                    imageSync.SavedFileAbsolutePaths.ForEach(_productVariantImageService.TryDeleteFile);
                    return BadRequest(Fail(imageSync.Message));
                }

                savedFiles.AddRange(imageSync.SavedFileAbsolutePaths);
                deferredDeleteFiles.AddRange(imageSync.DeferredDeleteAbsolutePaths);

                await tx.CommitAsync();
                deferredDeleteFiles.ForEach(_productVariantImageService.TryDeleteFile);

                var images = await GetImagesForSkuAsync(skuId);
                return Ok(new
                {
                    result.ResponseCode,
                    result.StatusCode,
                    result.ResponseMsg,
                    Images = images
                });
            }
            catch
            {
                await tx.RollbackAsync();
                savedFiles.ForEach(_productVariantImageService.TryDeleteFile);
                throw;
            }
        }

        [HttpPost]
        [Route("Update")]
        [RequestSizeLimit(20 * 1024 * 1024)]
        public async Task<IActionResult> Update([FromForm] ProductVariantUpdateInputVIEW viewInput)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new { message = "Validation failed.", errors });
            }

            var variantRows = ResolveVariantValues(viewInput);
            if (variantRows.Count == 0)
            {
                return BadRequest(new { message = "At least one variant value row is required." });
            }

            var input = new ProductVariantUpdateInput
            {
                ID_ProductVariant = viewInput.ID_ProductVariant,
                FK_Product = viewInput.FK_Product,
                SKU = viewInput.SKU,
                VariantLabel = viewInput.VariantLabel,
                MRP = viewInput.MRP,
                SellingPrice = viewInput.SellingPrice,
                IsActive = viewInput.IsActive,
                IsDefault = viewInput.IsDefault,
                VariantValues = variantRows
                    .Where(v => v.VariantId > 0 && v.VariantValueId > 0)
                    .Select(v => new ProductVariantValueRowInput
                    {
                        VariantId = v.VariantId,
                        VariantValueId = v.VariantValueId
                    })
                    .ToList(),
                EnterBy = 1
            };

            await using var tx = await _dbContext.Database.BeginTransactionAsync();
            var savedFiles = new List<string>();
            var deferredDeleteFiles = new List<string>();
            try
            {
                var result = await _productVariantInterface.UpdateProductVariantAsync(input);
                if (!result.StatusCode)
                {
                    await tx.RollbackAsync();
                    return BadRequest(result);
                }

                var skuId = viewInput.ID_ProductVariant;
                if (skuId <= 0)
                {
                    await tx.RollbackAsync();
                    return BadRequest(Fail("Invalid SKU."));
                }

                var imageSync = await SyncImagesInCreateUpdateAsync(viewInput, skuId);
                if (!imageSync.StatusCode)
                {
                    await tx.RollbackAsync();
                    imageSync.SavedFileAbsolutePaths.ForEach(_productVariantImageService.TryDeleteFile);
                    return BadRequest(Fail(imageSync.Message));
                }

                savedFiles.AddRange(imageSync.SavedFileAbsolutePaths);
                deferredDeleteFiles.AddRange(imageSync.DeferredDeleteAbsolutePaths);

                await tx.CommitAsync();
                deferredDeleteFiles.ForEach(_productVariantImageService.TryDeleteFile);

                var images = await GetImagesForSkuAsync(skuId);
                return Ok(new
                {
                    result.ResponseCode,
                    result.StatusCode,
                    result.ResponseMsg,
                    Images = images
                });
            }
            catch
            {
                await tx.RollbackAsync();
                savedFiles.ForEach(_productVariantImageService.TryDeleteFile);
                throw;
            }
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IActionResult> Delete([FromBody] ProductVariantDeleteInputVIEW viewInput)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new { message = "Validation failed.", errors });
            }

            var input = new ProductVariantDeleteInput
            {
                ID_ProductVariant = viewInput.ID_ProductVariant,
                EnterBy = 1
            };

            var result = await _productVariantInterface.DeleteProductVariantAsync(input);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetImages/{skuId}")]
        public async Task<IActionResult> GetImages(int skuId)
        {
            if (skuId <= 0)
            {
                return BadRequest(new { message = "Invalid SKU ID." });
            }

            var rows = await _productVariantImageRepository.GetBySkuIdAsync(skuId);
            var result = rows.Select(_productVariantImageService.MapToDto).ToList();
            return Ok(result);
        }

        [HttpPost]
        [Route("UploadImages")]
        [RequestSizeLimit(20 * 1024 * 1024)]
        public async Task<IActionResult> UploadImages([FromForm] ProductVariantImageUploadInput input)
        {
            var pathContext = await GetImagePathContextAsync(input.SKUId);
            if (pathContext == null)
            {
                return BadRequest(Fail("Invalid SKU."));
            }

            var validationError = _productVariantImageService.ValidateUploadInput(input);
            if (!string.IsNullOrWhiteSpace(validationError))
            {
                return BadRequest(Fail(validationError));
            }

            if (!await _productVariantImageRepository.ProductVariantExistsAsync(input.SKUId))
            {
                return BadRequest(Fail("Invalid SKU."));
            }

            var existing = await _productVariantImageRepository.GetBySkuIdAsync(input.SKUId);
            if (existing.Count + input.Files.Count > MaxFilesPerSku)
            {
                return BadRequest(Fail($"Maximum {MaxFilesPerSku} images are allowed per SKU."));
            }

            var uploadRoot = _productVariantImageService.GetUploadRoot(_environment.WebRootPath, pathContext.ProductSlug, pathContext.Sku);
            Directory.CreateDirectory(uploadRoot);

            var orderedExisting = _productVariantImageService.ReorderExisting(existing, input.ExistingImageOrder);
            for (var i = 0; i < orderedExisting.Count; i++)
            {
                orderedExisting[i].DisplayOrder = i;
            }

            var savedAbsolutePaths = new List<string>();
            var newRows = new List<ProductVariantImageEntity>();
            var nextOrder = orderedExisting.Count;
            var nextImageNumber = _productVariantImageService.GetNextImageNumber(uploadRoot);

            try
            {
                foreach (var file in input.Files)
                {
                    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                    var fileName = $"{nextImageNumber++}{ext}";
                    while (System.IO.File.Exists(Path.Combine(uploadRoot, fileName)))
                    {
                        fileName = $"{nextImageNumber++}{ext}";
                    }
                    var absolutePath = Path.Combine(uploadRoot, fileName);
                    await using (var stream = new FileStream(absolutePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    savedAbsolutePaths.Add(absolutePath);

                    newRows.Add(new ProductVariantImageEntity
                    {
                        FK_ProductVariant = input.SKUId,
                        ImageUrl = _productVariantImageService.BuildImageUrl(pathContext.ProductSlug, pathContext.Sku, fileName),
                        IsPrimary = false,
                        DisplayOrder = nextOrder++,
                        CreatedAt = DateTime.Now
                    });
                }

                if (input.PrimaryIndex.HasValue &&
                    input.PrimaryIndex.Value >= 0 &&
                    input.PrimaryIndex.Value < newRows.Count)
                {
                    foreach (var row in orderedExisting)
                    {
                        row.IsPrimary = false;
                    }

                    for (var i = 0; i < newRows.Count; i++)
                    {
                        newRows[i].IsPrimary = i == input.PrimaryIndex!.Value;
                    }
                }
                else if (!orderedExisting.Any(x => x.IsPrimary) && newRows.Count > 0)
                {
                    newRows[0].IsPrimary = true;
                }

                await _productVariantImageRepository.AddRangeAsync(newRows);
                await _productVariantImageRepository.SaveChangesAsync();

                var response = Ok(input.SKUId, "Images uploaded successfully.");
                var rows = await _productVariantImageRepository.GetBySkuIdAsync(input.SKUId);
                var images = rows.Select(_productVariantImageService.MapToDto).ToList();

                return Ok(new
                {
                    response.ResponseCode,
                    response.StatusCode,
                    response.ResponseMsg,
                    Images = images
                });
            }
            catch (Exception ex)
            {
                foreach (var path in savedAbsolutePaths)
                {
                    _productVariantImageService.TryDeleteFile(path);
                }

                _logger.LogError(ex, "Failed uploading images for SKU {SkuId}", input.SKUId);
                throw;
            }
        }

        [HttpPost]
        [Route("DeleteImage")]
        public async Task<IActionResult> DeleteImage([FromBody] ProductVariantImageDeleteInput input)
        {
            if (input == null || input.ImageId <= 0)
            {
                return BadRequest(Fail("Invalid image."));
            }

            var image = await _productVariantImageRepository.GetByIdAsync(input.ImageId);
            if (image == null)
            {
                return BadRequest(Fail("Image not found."));
            }

            var all = await _productVariantImageRepository.GetBySkuIdAsync(image.FK_ProductVariant);
            if (all.Count <= 1)
            {
                return BadRequest(Fail("At least one image is required for this SKU."));
            }

            await _productVariantImageRepository.RemoveAsync(image);

            var remaining = all.Where(x => x.ID_ProductVariantImage != image.ID_ProductVariantImage).ToList();
            for (var i = 0; i < remaining.Count; i++)
            {
                remaining[i].DisplayOrder = i;
            }

            if (image.IsPrimary && remaining.Count > 0 && !remaining.Any(x => x.IsPrimary))
            {
                remaining[0].IsPrimary = true;
            }

            await _productVariantImageRepository.SaveChangesAsync();

            var absolutePath = Path.Combine(_environment.WebRootPath, image.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            _productVariantImageService.TryDeleteFile(absolutePath);

            return Ok(Ok(image.ID_ProductVariantImage, "Image deleted successfully."));
        }

        [HttpPost]
        [Route("SetPrimaryImage")]
        public async Task<IActionResult> SetPrimaryImage([FromBody] ProductVariantImageSetPrimaryInput input)
        {
            if (input == null || input.SKUId <= 0 || input.ImageId <= 0)
            {
                return BadRequest(Fail("Invalid request."));
            }

            var images = await _productVariantImageRepository.GetBySkuIdAsync(input.SKUId);
            if (images.Count == 0)
            {
                return BadRequest(Fail("No images found for this SKU."));
            }

            if (!images.Any(x => x.ID_ProductVariantImage == input.ImageId))
            {
                return BadRequest(Fail("Primary image does not belong to this SKU."));
            }

            var ordered = _productVariantImageService.ReorderExisting(images, input.OrderedImageIds);
            for (var i = 0; i < ordered.Count; i++)
            {
                ordered[i].DisplayOrder = i;
                ordered[i].IsPrimary = ordered[i].ID_ProductVariantImage == input.ImageId;
            }

            await _productVariantImageRepository.SaveChangesAsync();

            var response = Ok(input.ImageId, "Primary image updated successfully.");
            var rows = await _productVariantImageRepository.GetBySkuIdAsync(input.SKUId);
            var resultImages = rows.Select(_productVariantImageService.MapToDto).ToList();
            return Ok(new
            {
                response.ResponseCode,
                response.StatusCode,
                response.ResponseMsg,
                Images = resultImages
            });
        }

        private static CommonResponse Ok(long responseCode, string message) =>
            new()
            {
                ResponseCode = responseCode,
                StatusCode = true,
                ResponseMsg = message
            };

        private static CommonResponse Fail(string message) =>
            new()
            {
                ResponseCode = -1,
                StatusCode = false,
                ResponseMsg = message
            };

        private static List<VariantValueRowVIEW> ResolveVariantValues(ProductVariantUpdateInputVIEW viewInput)
        {
            if (viewInput.VariantValues != null && viewInput.VariantValues.Count > 0)
            {
                return viewInput.VariantValues;
            }

            if (string.IsNullOrWhiteSpace(viewInput.VariantValuesJson))
            {
                return new List<VariantValueRowVIEW>();
            }

            try
            {
                return JsonSerializer.Deserialize<List<VariantValueRowVIEW>>(viewInput.VariantValuesJson) ?? new List<VariantValueRowVIEW>();
            }
            catch
            {
                return new List<VariantValueRowVIEW>();
            }
        }

        private async Task<List<ProductVariantImageDto>> GetImagesForSkuAsync(int skuId)
        {
            var rows = await _productVariantImageRepository.GetBySkuIdAsync(skuId);
            return rows.Select(_productVariantImageService.MapToDto).ToList();
        }

        private async Task<ImageSyncResult> SyncImagesInCreateUpdateAsync(ProductVariantUpdateInputVIEW viewInput, int skuId)
        {
            var result = new ImageSyncResult();
            var pathContext = await GetImagePathContextAsync(skuId);
            if (pathContext == null)
            {
                return ImageSyncResult.Fail("Invalid SKU.");
            }

            var existing = await _productVariantImageRepository.GetBySkuIdAsync(skuId);
            var removedIds = (viewInput.RemovedImageIds ?? new List<int>()).Where(x => x > 0).Distinct().ToHashSet();

            var orderedExisting = _productVariantImageService.ReorderExisting(existing, viewInput.ExistingImageOrder);
            orderedExisting = orderedExisting
                .Where(x => !removedIds.Contains(x.ID_ProductVariantImage))
                .ToList();

            foreach (var row in existing.Where(x => removedIds.Contains(x.ID_ProductVariantImage)))
            {
                await _productVariantImageRepository.RemoveAsync(row);
                var removePath = Path.Combine(_environment.WebRootPath, row.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                result.DeferredDeleteAbsolutePaths.Add(removePath);
            }

            var newRows = new List<ProductVariantImageEntity>();
            var incomingFiles = viewInput.Files ?? new List<IFormFile>();
            var uploadRoot = _productVariantImageService.GetUploadRoot(_environment.WebRootPath, pathContext.ProductSlug, pathContext.Sku);
            Directory.CreateDirectory(uploadRoot);
            var nextImageNumber = _productVariantImageService.GetNextImageNumber(uploadRoot);

            if (orderedExisting.Count + incomingFiles.Count > MaxFilesPerSku)
            {
                return ImageSyncResult.Fail($"Maximum {MaxFilesPerSku} images are allowed per SKU.");
            }

            foreach (var file in incomingFiles)
            {
                var uploadValidation = _productVariantImageService.ValidateUploadInput(new ProductVariantImageUploadInput
                {
                    SKUId = skuId,
                    Files = new List<IFormFile> { file }
                });
                if (!string.IsNullOrWhiteSpace(uploadValidation))
                {
                    return ImageSyncResult.Fail(uploadValidation);
                }

                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                var fileName = $"{nextImageNumber++}{ext}";
                while (System.IO.File.Exists(Path.Combine(uploadRoot, fileName)))
                {
                    fileName = $"{nextImageNumber++}{ext}";
                }
                var absolutePath = Path.Combine(uploadRoot, fileName);
                await using (var stream = new FileStream(absolutePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                result.SavedFileAbsolutePaths.Add(absolutePath);
                newRows.Add(new ProductVariantImageEntity
                {
                    FK_ProductVariant = skuId,
                    ImageUrl = _productVariantImageService.BuildImageUrl(pathContext.ProductSlug, pathContext.Sku, fileName),
                    IsPrimary = false,
                    DisplayOrder = 0,
                    CreatedAt = DateTime.Now
                });
            }

            var finalRows = new List<ProductVariantImageEntity>();
            finalRows.AddRange(orderedExisting);
            finalRows.AddRange(newRows);
            var existingPrimaryId = orderedExisting.FirstOrDefault(x => x.IsPrimary)?.ID_ProductVariantImage;

            if (finalRows.Count == 0)
            {
                return ImageSyncResult.Fail("At least one image is required for this SKU.");
            }

            foreach (var row in finalRows)
            {
                row.IsPrimary = false;
            }

            if (viewInput.PrimaryImageId.HasValue && viewInput.PrimaryImageId.Value > 0)
            {
                var target = finalRows.FirstOrDefault(x => x.ID_ProductVariantImage == viewInput.PrimaryImageId.Value);
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
                var originalPrimary = existingPrimaryId.HasValue
                    ? orderedExisting.FirstOrDefault(x => x.ID_ProductVariantImage == existingPrimaryId.Value)
                    : null;
                if (originalPrimary != null)
                {
                    originalPrimary.IsPrimary = true;
                }
            }

            if (!finalRows.Any(x => x.IsPrimary))
            {
                finalRows[0].IsPrimary = true;
            }

            for (var i = 0; i < finalRows.Count; i++)
            {
                finalRows[i].DisplayOrder = i;
            }

            if (newRows.Count > 0)
            {
                await _productVariantImageRepository.AddRangeAsync(newRows);
            }

            await _productVariantImageRepository.SaveChangesAsync();
            return result;
        }

        private sealed class ImageSyncResult
        {
            public bool StatusCode { get; set; } = true;
            public string Message { get; set; } = string.Empty;
            public List<string> SavedFileAbsolutePaths { get; } = new();
            public List<string> DeferredDeleteAbsolutePaths { get; } = new();

            public static ImageSyncResult Fail(string message) =>
                new()
                {
                    StatusCode = false,
                    Message = message
                };
        }

        private async Task<ImagePathContext?> GetImagePathContextAsync(int skuId)
        {
            var row = await (
                from pv in _dbContext.ProductVariants.AsNoTracking()
                join p in _dbContext.Products.AsNoTracking() on pv.FK_Product equals p.ID_Product
                where pv.ID_ProductVariant == skuId && !pv.Cancelled && !p.Cancelled
                select new { p.Slug, pv.SKU }
            ).FirstOrDefaultAsync();

            if (row == null || string.IsNullOrWhiteSpace(row.Slug) || string.IsNullOrWhiteSpace(row.SKU))
            {
                return null;
            }

            return new ImagePathContext(row.Slug, row.SKU);
        }

        private sealed record ImagePathContext(string ProductSlug, string Sku);
    }
}
