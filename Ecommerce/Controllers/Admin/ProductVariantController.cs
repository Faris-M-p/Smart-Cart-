using Ecommerce.Interface.Admin;
using Ecommerce.Models.Entities;
using Ecommerce.Services.Admin;
using Microsoft.AspNetCore.Mvc;
using static Ecommerce.Models.Admin.ProductVariantModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Controllers.Admin
{
    [Route("Admin/ProductVariant")]
    public class ProductVariantController : Controller
    {
        private const int MaxFilesPerSku = 5;
        private readonly IProductVariantInterface _productVariantInterface;
        private readonly IProductVariantImageRepository _productVariantImageRepository;
        private readonly ProductVariantImageService _productVariantImageService;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ProductVariantController> _logger;

        public ProductVariantController(
            IProductVariantInterface productVariantInterface,
            IProductVariantImageRepository productVariantImageRepository,
            ProductVariantImageService productVariantImageService,
            IWebHostEnvironment environment,
            ILogger<ProductVariantController> logger)
        {
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
        public async Task<IActionResult> Create([FromBody] ProductVariantUpdateInputVIEW viewInput)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new { message = "Validation failed.", errors });
            }

            if (viewInput.VariantValues == null || viewInput.VariantValues.Count == 0)
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
                VariantValues = viewInput.VariantValues
                    .Where(v => v.VariantId > 0 && v.VariantValueId > 0)
                    .Select(v => new ProductVariantValueRowInput
                    {
                        VariantId = v.VariantId,
                        VariantValueId = v.VariantValueId
                    })
                    .ToList(),
                EnterBy = 1
            };

            var result = await _productVariantInterface.CreateProductVariantAsync(input);
            return Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IActionResult> Update([FromBody] ProductVariantUpdateInputVIEW viewInput)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new { message = "Validation failed.", errors });
            }

            if (viewInput.VariantValues == null || viewInput.VariantValues.Count == 0)
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
                VariantValues = viewInput.VariantValues
                    .Where(v => v.VariantId > 0 && v.VariantValueId > 0)
                    .Select(v => new ProductVariantValueRowInput
                    {
                        VariantId = v.VariantId,
                        VariantValueId = v.VariantValueId
                    })
                    .ToList(),
                EnterBy = 1
            };

            var result = await _productVariantInterface.UpdateProductVariantAsync(input);
            return Ok(result);
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

            var uploadRoot = _productVariantImageService.GetUploadRoot(_environment.WebRootPath);
            Directory.CreateDirectory(uploadRoot);

            var orderedExisting = _productVariantImageService.ReorderExisting(existing, input.ExistingImageOrder);
            for (var i = 0; i < orderedExisting.Count; i++)
            {
                orderedExisting[i].DisplayOrder = i;
            }

            var savedAbsolutePaths = new List<string>();
            var newRows = new List<ProductVariantImageEntity>();
            var nextOrder = orderedExisting.Count;

            try
            {
                foreach (var file in input.Files)
                {
                    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                    var fileName = $"{Guid.NewGuid():N}{ext}";
                    var absolutePath = Path.Combine(uploadRoot, fileName);
                    await using (var stream = new FileStream(absolutePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    savedAbsolutePaths.Add(absolutePath);

                    newRows.Add(new ProductVariantImageEntity
                    {
                        FK_ProductVariant = input.SKUId,
                        ImageUrl = _productVariantImageService.BuildImageUrl(fileName),
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
                return BadRequest(Fail("Failed to upload images."));
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
    }
}
