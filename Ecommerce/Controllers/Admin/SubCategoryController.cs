using Microsoft.AspNetCore.Mvc;
using Ecommerce.Filters;
using Ecommerce.Helpers.Common;
using Ecommerce.Interface.Admin;
using static Ecommerce.Models.Admin.SubCategoryModel;
using static Ecommerce.Models.Admin.CategoryModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Controllers.Admin
{
    [Route("Admin/SubCategory")]
    public class SubCategoryController : Controller
    {
        private readonly ISubCategoryInterface _subCategoryInterface;
        private readonly ICategoryInterface _categoryInterface;
        private readonly CommonImageService _commonImageService;
        private readonly IWebHostEnvironment _environment;

        public SubCategoryController(
            ISubCategoryInterface subCategoryInterface,
            ICategoryInterface categoryInterface,
            CommonImageService commonImageService,
            IWebHostEnvironment environment)
        {
            _subCategoryInterface = subCategoryInterface;
            _categoryInterface = categoryInterface;
            _commonImageService = commonImageService;
            _environment = environment;
        }

        [Route("")]
        [Route("Index")]
        [RequirePermission("SubCategories.View")]
        public IActionResult Index()
        {
            return View("~/Views/Admin/SubCategory/Index.cshtml");
        }

        [HttpPost]
        [Route("GetSubCategoryList")]
        [RequirePermission("SubCategories.View")]
        public async Task<IActionResult> GetSubCategoryList([FromBody] SubCategoryListInputVIEW viewInput)
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
                var input = new SubCategoryListInput
                {
                    SearchText = viewInput.SearchText,
                    FilterCategoryIDs = viewInput.FilterCategoryIDs,
                    FilterSubCategoryIDs = viewInput.FilterSubCategoryIDs,
                    PageIndex = viewInput.PageIndex,
                    PageSize = viewInput.PageSize,
                    SortColumn = viewInput.SortColumn,
                    SortMode = viewInput.SortMode
                };

                var result = await _subCategoryInterface.GetSubCategoryListAsync(input);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost]
        [Route("Create")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        [RequirePermission("SubCategories.Create")]
        public async Task<IActionResult> Create([FromForm] SubCategoryUpdateInputVIEW viewInput)
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

                if (viewInput.SubCategoryImage != null)
                {
                    var imageError = _commonImageService.ValidateImageFile(viewInput.SubCategoryImage);
                    if (!string.IsNullOrWhiteSpace(imageError))
                    {
                        return BadRequest(new { message = imageError });
                    }
                }

                var input = new SubCategoryUpdateInput
                {
                    UserAction = 1,
                    SubCategoryID = viewInput.SubCategoryID,
                    SubCategoryName = viewInput.SubCategoryName,
                    FK_Category = viewInput.FK_Category,
                    Description = viewInput.Description,
                    IsActive = viewInput.IsActive ?? true,
                    EnterBy = 1
                };

                var result = await _subCategoryInterface.CreateSubCategoryAsync(input);
                if (result.StatusCode && (viewInput.SubCategoryImage != null || viewInput.RemoveImage))
                {
                    var subCategoryId = (int)result.ResponseCode;
                    var imageUrl = await _commonImageService.SaveSingleImageAsync(
                        viewInput.SubCategoryImage,
                        _environment.WebRootPath,
                        "subcategories",
                        subCategoryId);
                    await _subCategoryInterface.SetImageUrlAsync(subCategoryId, imageUrl);
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
        [RequirePermission("SubCategories.Edit")]
        public async Task<IActionResult> Update([FromForm] SubCategoryUpdateInputVIEW viewInput)
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

                if (viewInput.SubCategoryImage != null)
                {
                    var imageError = _commonImageService.ValidateImageFile(viewInput.SubCategoryImage);
                    if (!string.IsNullOrWhiteSpace(imageError))
                    {
                        return BadRequest(new { message = imageError });
                    }
                }

                var input = new SubCategoryUpdateInput
                {
                    UserAction = 2,
                    SubCategoryID = viewInput.SubCategoryID,
                    SubCategoryName = viewInput.SubCategoryName,
                    FK_Category = viewInput.FK_Category,
                    Description = viewInput.Description,
                    IsActive = viewInput.IsActive ?? true,
                    EnterBy = 1
                };

                var result = await _subCategoryInterface.UpdateSubCategoryAsync(input);
                if (result.StatusCode && (viewInput.SubCategoryImage != null || viewInput.RemoveImage))
                {
                    var imageUrl = await _commonImageService.SaveSingleImageAsync(
                        viewInput.SubCategoryImage,
                        _environment.WebRootPath,
                        "subcategories",
                        viewInput.SubCategoryID);
                    await _subCategoryInterface.SetImageUrlAsync(viewInput.SubCategoryID, imageUrl);
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
        [RequirePermission("SubCategories.Delete")]
        public async Task<IActionResult> Delete([FromBody] SubCategoryDeleteInputVIEW viewInput)
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
                var input = new SubCategoryDeleteInput
                {
                    SubCategoryID = viewInput.SubCategoryID,
                    CancelledReason = viewInput.CancelledReason,
                    EnterBy = 1 // TODO: Get from session/auth
                };

                var result = await _subCategoryInterface.DeleteSubCategoryAsync(input);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        [HttpGet]
        [Route("GetById/{id}")]
        [RequirePermission("SubCategories.View")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var row = await _subCategoryInterface.GetSubCategoryByIdAsync(id);
                if (row == null)
                {
                    return NotFound(new { message = "SubCategory not found." });
                }

                return Ok(row);
            }
            catch
            {
                throw;
            }
        }

        [HttpGet]
        [Route("GetCategories")]
        [RequirePermission("SubCategories.View")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var rows = await _categoryInterface.GetActiveCategoriesAsync();
                return Ok(rows);
            }
            catch
            {
                throw;
            }
        }
    }
}
