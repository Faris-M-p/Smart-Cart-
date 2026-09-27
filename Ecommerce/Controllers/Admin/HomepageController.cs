using Microsoft.AspNetCore.Mvc;
using Ecommerce.Filters;
using Ecommerce.Helpers.Common;
using Ecommerce.Interface.Admin;
using static Ecommerce.Models.Admin.HomepageModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Controllers.Admin
{
    [Route("Admin/Homepage")]
    public class HomepageController : Controller
    {
        private readonly IHomepageInterface _homepageInterface;
        private readonly CommonImageService _commonImageService;
        private readonly IWebHostEnvironment _environment;

        public HomepageController(
            IHomepageInterface homepageInterface,
            CommonImageService commonImageService,
            IWebHostEnvironment environment)
        {
            _homepageInterface = homepageInterface;
            _commonImageService = commonImageService;
            _environment = environment;
        }

        [Route("")]
        [Route("Index")]
        [RequirePermission("Homepage.View")]
        public IActionResult Index()
        {
            return View("~/Views/Admin/Homepage/Index.cshtml");
        }

        // =========================================================================
        // Banners
        // =========================================================================
        [HttpGet("GetBanners")]
        [RequirePermission("Homepage.View")]
        public async Task<IActionResult> GetBanners()
        {
            var data = await _homepageInterface.GetBannersAsync();
            return Ok(new ApiResponse<List<BannerDto>> { Success = true, Data = data });
        }

        [HttpGet("GetBanner/{id}")]
        [RequirePermission("Homepage.View")]
        public async Task<IActionResult> GetBanner(int id)
        {
            var banner = await _homepageInterface.GetBannerByIdAsync(id);
            if (banner == null) return NotFound(new ApiResponse<string> { Success = false, Message = "Banner not found" });
            return Ok(new ApiResponse<BannerDto> { Success = true, Data = banner });
        }

        [HttpPost("SaveBanner")]
        [RequirePermission("Homepage.Edit")]
        public async Task<IActionResult> SaveBanner([FromForm] BannerSaveInputVIEW viewInput)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Validation failed", Errors = errors });
                }

                if (viewInput.BannerImage != null)
                {
                    var imageError = _commonImageService.ValidateImageFile(viewInput.BannerImage);
                    if (!string.IsNullOrWhiteSpace(imageError))
                    {
                        return BadRequest(new ApiResponse<string> { Success = false, Message = imageError });
                    }
                }

                var input = new BannerSaveInput
                {
                    BannerId = viewInput.BannerId,
                    Title = viewInput.Title ?? string.Empty,
                    Subtitle = viewInput.Subtitle,
                    TargetUrl = viewInput.TargetUrl,
                    DisplayOrder = viewInput.DisplayOrder,
                    IsActive = viewInput.IsActive ?? true,
                    CategoryIds = viewInput.CategoryIds ?? new List<int>()
                };

                CommonResponse result;
                if (input.BannerId > 0)
                {
                    result = await _homepageInterface.UpdateBannerAsync(input);
                }
                else
                {
                    result = await _homepageInterface.CreateBannerAsync(input);
                }

                if (result.StatusCode)
                {
                    int bannerId = (int)result.ResponseCode;

                    if (viewInput.BannerImage != null || viewInput.RemoveImage)
                    {
                        var imageUrl = await _commonImageService.SaveSingleImageAsync(
                            viewInput.BannerImage,
                            _environment.WebRootPath,
                            "banners",
                            bannerId);
                        await _homepageInterface.SetBannerImageUrlAsync(bannerId, imageUrl);
                    }
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string> { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("DeleteBanner/{id}")]
        [RequirePermission("Homepage.Edit")]
        public async Task<IActionResult> DeleteBanner(int id)
        {
            var result = await _homepageInterface.DeleteBannerAsync(id);
            return Ok(result);
        }

        // =========================================================================
        // Featured Categories
        // =========================================================================
        [HttpGet("GetCategories")]
        [RequirePermission("Homepage.View")]
        public async Task<IActionResult> GetCategories()
        {
            var data = await _homepageInterface.GetFeaturedCategoriesAsync();
            return Ok(new ApiResponse<List<FeaturedCategoryDto>> { Success = true, Data = data });
        }

        [HttpPost("SaveCategory")]
        [RequirePermission("Homepage.Edit")]
        public async Task<IActionResult> SaveCategory([FromBody] FeaturedCategorySaveInputVIEW input)
        {
            var result = await _homepageInterface.SaveFeaturedCategoryAsync(input);
            return Ok(result);
        }

        [HttpPost("ToggleCategoryStatus/{id}")]
        [RequirePermission("Homepage.Edit")]
        public async Task<IActionResult> ToggleCategoryStatus(int id)
        {
            var result = await _homepageInterface.ToggleFeaturedCategoryStatusAsync(id);
            return Ok(result);
        }

        [HttpPost("UpdateCategoryOrder")]
        [RequirePermission("Homepage.Edit")]
        public async Task<IActionResult> UpdateCategoryOrder([FromBody] OrderUpdateInput input)
        {
            var result = await _homepageInterface.UpdateFeaturedCategoryOrderAsync(input.Id, input.DisplayOrder);
            return Ok(result);
        }

        [HttpPost("DeleteCategory/{id}")]
        [RequirePermission("Homepage.Edit")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var result = await _homepageInterface.DeleteFeaturedCategoryAsync(id);
            return Ok(result);
        }

        // =========================================================================
        // Featured Products
        // =========================================================================
        [HttpGet("GetProducts")]
        [RequirePermission("Homepage.View")]
        public async Task<IActionResult> GetProducts()
        {
            var data = await _homepageInterface.GetFeaturedProductsAsync();
            return Ok(new ApiResponse<List<FeaturedProductDto>> { Success = true, Data = data });
        }

        [HttpPost("SaveProduct")]
        [RequirePermission("Homepage.Edit")]
        public async Task<IActionResult> SaveProduct([FromBody] FeaturedProductSaveInputVIEW input)
        {
            var result = await _homepageInterface.SaveFeaturedProductAsync(input);
            return Ok(result);
        }

        [HttpPost("ToggleProductStatus/{id}")]
        [RequirePermission("Homepage.Edit")]
        public async Task<IActionResult> ToggleProductStatus(int id)
        {
            var result = await _homepageInterface.ToggleFeaturedProductStatusAsync(id);
            return Ok(result);
        }

        [HttpPost("UpdateProductOrder")]
        [RequirePermission("Homepage.Edit")]
        public async Task<IActionResult> UpdateProductOrder([FromBody] OrderUpdateInput input)
        {
            var result = await _homepageInterface.UpdateFeaturedProductOrderAsync(input.Id, input.DisplayOrder);
            return Ok(result);
        }

        [HttpPost("DeleteProduct/{id}")]
        [RequirePermission("Homepage.Edit")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var result = await _homepageInterface.DeleteFeaturedProductAsync(id);
            return Ok(result);
        }

        // =========================================================================
        // Lookups
        // =========================================================================
        [HttpGet("GetAvailableCategoryOptions")]
        [RequirePermission("Homepage.View")]
        public async Task<IActionResult> GetAvailableCategoryOptions()
        {
            var data = await _homepageInterface.GetAvailableCategoryOptionsAsync();
            return Ok(new ApiResponse<List<CategoryOption>> { Success = true, Data = data });
        }

        [HttpGet("GetAvailableProductOptions")]
        [RequirePermission("Homepage.View")]
        public async Task<IActionResult> GetAvailableProductOptions()
        {
            var data = await _homepageInterface.GetAvailableProductOptionsAsync();
            return Ok(new ApiResponse<List<ProductOption>> { Success = true, Data = data });
        }
    }

    public class OrderUpdateInput
    {
        public int Id { get; set; }
        public int DisplayOrder { get; set; }
    }
}
