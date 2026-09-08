using Microsoft.AspNetCore.Mvc;
using Ecommerce.Filters;
using Ecommerce.Helpers.Common;
using Ecommerce.Interface.Admin;
using static Ecommerce.Models.Admin.CategoryModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Controllers.Admin
{
    [Route("Admin/Category")]
    public class CategoryController : Controller
    {
        private readonly ICategoryInterface _categoryInterface;
        private readonly CommonImageService _commonImageService;
        private readonly IWebHostEnvironment _environment;

        public CategoryController(
            ICategoryInterface categoryInterface,
            CommonImageService commonImageService,
            IWebHostEnvironment environment)
        {
            _categoryInterface = categoryInterface;
            _commonImageService = commonImageService;
            _environment = environment;
        }

        [Route("")]
        [Route("Index")]
        [RequirePermission("Categories.View")]
        public IActionResult Index()
        {
            return View("~/Views/Admin/Category/Index.cshtml");
        }

        [HttpPost]
        [Route("GetCategoryList")]
        [RequirePermission("Categories.View")]
        public async Task<IActionResult> GetCategoryList(
            [FromBody] CategoryListInputVIEW viewInput)
        {
            try
            {
                //----------------------------------
                // VALIDATION
                //----------------------------------
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new ApiResponse<TableOutput<Category>>
                    {
                        Success = false,
                        Message = "Validation failed",
                        Errors = errors
                    });
                }

                //----------------------------------
                // MAPPING
                //----------------------------------
                var input = new CategoryListInput
                {
                    SearchText = viewInput.SearchText,
                    FilterCategoryIDs = viewInput.FilterCategoryIDs,
                    PageIndex = viewInput.PageIndex,
                    PageSize = viewInput.PageSize,
                    SortColumn = viewInput.SortColumn,
                    SortMode = viewInput.SortMode
                };

                //----------------------------------
                // SERVICE CALL
                //----------------------------------
                var result = await _categoryInterface
                    .GetCategoryListAsync(input);

                //----------------------------------
                // SUCCESS
                //----------------------------------
                return Ok(new ApiResponse<TableOutput<Category>>
                {
                    Success = true,
                    Message = "Categories loaded successfully",
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
        [RequirePermission("Categories.Create")]
        public async Task<IActionResult> Create([FromForm] CategoryUpdateInputVIEW viewInput)
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

                if (viewInput.CategoryImage != null)
                {
                    var imageError = _commonImageService.ValidateImageFile(viewInput.CategoryImage);
                    if (!string.IsNullOrWhiteSpace(imageError))
                    {
                        return BadRequest(new { message = imageError });
                    }
                }

                var input = new CategoryUpdateInput
                {
                    UserAction = 1,
                    CategoryID = viewInput.CategoryID,
                    CategoryName = viewInput.CategoryName,
                    Description = viewInput.Description,
                    IsActive = viewInput.IsActive ?? true,
                    EnterBy = 1
                };

                var result = await _categoryInterface.CreateCategoryAsync(input);
                if (result.StatusCode && (viewInput.CategoryImage != null || viewInput.RemoveImage))
                {
                    var categoryId = (int)result.ResponseCode;
                    var imageUrl = await _commonImageService.SaveSingleImageAsync(
                        viewInput.CategoryImage,
                        _environment.WebRootPath,
                        "categories",
                        categoryId);
                    await _categoryInterface.SetImageUrlAsync(categoryId, imageUrl);
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
        [RequirePermission("Categories.Edit")]
        public async Task<IActionResult> Update([FromForm] CategoryUpdateInputVIEW viewInput)
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

                if (viewInput.CategoryImage != null)
                {
                    var imageError = _commonImageService.ValidateImageFile(viewInput.CategoryImage);
                    if (!string.IsNullOrWhiteSpace(imageError))
                    {
                        return BadRequest(new { message = imageError });
                    }
                }

                var input = new CategoryUpdateInput
                {
                    UserAction = 2,
                    CategoryID = viewInput.CategoryID,
                    CategoryName = viewInput.CategoryName,
                    Description = viewInput.Description,
                    IsActive = viewInput.IsActive ?? true,
                    EnterBy = 1
                };

                var result = await _categoryInterface.UpdateCategoryAsync(input);
                if (result.StatusCode && (viewInput.CategoryImage != null || viewInput.RemoveImage))
                {
                    var imageUrl = await _commonImageService.SaveSingleImageAsync(
                        viewInput.CategoryImage,
                        _environment.WebRootPath,
                        "categories",
                        viewInput.CategoryID);
                    await _categoryInterface.SetImageUrlAsync(viewInput.CategoryID, imageUrl);
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
        [RequirePermission("Categories.Delete")]
        public async Task<IActionResult> Delete([FromBody] CategoryDeleteInputVIEW viewInput)
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

                // Map VIEW model to Procedure Input model
                var input = new CategoryDeleteInput
                {
                    CategoryID = viewInput.CategoryID,
                    CancelledReason = viewInput.CancelledReason,
                    EnterBy = 1 // TODO: Get from session/auth
                };

                var result = await _categoryInterface.DeleteCategoryAsync(input);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        [HttpGet]
        [Route("GetById/{id}")]
        [RequirePermission("Categories.View")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var row = await _categoryInterface.GetCategoryByIdAsync(id);
                if (row == null)
                {
                    return NotFound(new { message = "Category not found." });
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
