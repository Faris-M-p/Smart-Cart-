using Microsoft.AspNetCore.Mvc;
using Ecommerce.Filters;
using Ecommerce.Helpers.Common;
using Ecommerce.Interface.Admin;
using static Ecommerce.Models.Admin.BrandModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Controllers.Admin
{
    [Route("Admin/Brand")]
    public class BrandController : Controller
    {
        private readonly IBrandInterface _brandInterface;
        private readonly CommonImageService _commonImageService;
        private readonly IWebHostEnvironment _environment;

        public BrandController(
            IBrandInterface brandInterface,
            CommonImageService commonImageService,
            IWebHostEnvironment environment)
        {
            _brandInterface = brandInterface;
            _commonImageService = commonImageService;
            _environment = environment;
        }

        [Route("")]
        [Route("Index")]
        [RequirePermission("Brands.View")]
        public IActionResult Index()
        {
            return View("~/Views/Admin/Brand/Index.cshtml");
        }

        [HttpPost]
        [Route("GetBrandList")]
        [RequirePermission("Brands.View")]
        public async Task<IActionResult> GetBrandList([FromBody] BrandListInputVIEW viewInput)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new ApiResponse<TableOutput<Brand>>
                    {
                        Success = false,
                        Message = "Validation failed",
                        Errors = errors
                    });
                }

                var input = new BrandListInput
                {
                    SearchText = viewInput.SearchText,
                    FilterBrandIDs = viewInput.FilterBrandIDs,
                    PageIndex = viewInput.PageIndex,
                    PageSize = viewInput.PageSize,
                    SortColumn = viewInput.SortColumn,
                    SortMode = viewInput.SortMode
                };

                var result = await _brandInterface.GetBrandListAsync(input);

                return Ok(new ApiResponse<TableOutput<Brand>>
                {
                    Success = true,
                    Message = "Brands loaded successfully",
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
        [RequestSizeLimit(5 * 1024 * 1024)]
        [RequirePermission("Brands.Create")]
        public async Task<IActionResult> Create([FromForm] BrandUpdateInputVIEW viewInput)
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

                if (viewInput.BrandImage != null)
                {
                    var imageError = _commonImageService.ValidateImageFile(viewInput.BrandImage);
                    if (!string.IsNullOrWhiteSpace(imageError))
                    {
                        return BadRequest(new { message = imageError });
                    }
                }

                var input = new BrandUpdateInput
                {
                    UserAction = 1,
                    BrandID = viewInput.BrandID,
                    BrandName = viewInput.BrandName,
                    Description = viewInput.Description,
                    IsActive = viewInput.IsActive ?? true,
                    EnterBy = 1
                };

                var result = await _brandInterface.CreateBrandAsync(input);
                if (result.StatusCode && (viewInput.BrandImage != null || viewInput.RemoveImage))
                {
                    var brandId = (int)result.ResponseCode;
                    var imageUrl = await _commonImageService.SaveSingleImageAsync(
                        viewInput.BrandImage,
                        _environment.WebRootPath,
                        "brands",
                        brandId);
                    await _brandInterface.SetImageUrlAsync(brandId, imageUrl);
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
        [RequestSizeLimit(5 * 1024 * 1024)]
        [RequirePermission("Brands.Edit")]
        public async Task<IActionResult> Update([FromForm] BrandUpdateInputVIEW viewInput)
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

                if (viewInput.BrandImage != null)
                {
                    var imageError = _commonImageService.ValidateImageFile(viewInput.BrandImage);
                    if (!string.IsNullOrWhiteSpace(imageError))
                    {
                        return BadRequest(new { message = imageError });
                    }
                }

                var input = new BrandUpdateInput
                {
                    UserAction = 2,
                    BrandID = viewInput.BrandID,
                    BrandName = viewInput.BrandName,
                    Description = viewInput.Description,
                    IsActive = viewInput.IsActive ?? true,
                    EnterBy = 1
                };

                var result = await _brandInterface.UpdateBrandAsync(input);
                if (result.StatusCode && (viewInput.BrandImage != null || viewInput.RemoveImage))
                {
                    var imageUrl = await _commonImageService.SaveSingleImageAsync(
                        viewInput.BrandImage,
                        _environment.WebRootPath,
                        "brands",
                        viewInput.BrandID);
                    await _brandInterface.SetImageUrlAsync(viewInput.BrandID, imageUrl);
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
        [RequirePermission("Brands.Delete")]
        public async Task<IActionResult> Delete([FromBody] BrandDeleteInputVIEW viewInput)
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

                var input = new BrandDeleteInput
                {
                    BrandID = viewInput.BrandID,
                    CancelledReason = viewInput.CancelledReason,
                    EnterBy = 1
                };

                var result = await _brandInterface.DeleteBrandAsync(input);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        [HttpGet]
        [Route("GetById/{id}")]
        [RequirePermission("Brands.View")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var row = await _brandInterface.GetBrandByIdAsync(id);
                if (row == null)
                {
                    return NotFound(new { message = "Brand not found." });
                }

                return Ok(row);
            }
            catch
            {
                throw;
            }
        }
    }
}
