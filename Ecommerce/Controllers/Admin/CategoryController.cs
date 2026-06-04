using Microsoft.AspNetCore.Mvc;
using Ecommerce.Interface.Admin;
using static Ecommerce.Models.Admin.CategoryModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Controllers.Admin
{
    [Route("Admin/Category")]
    public class CategoryController : Controller
    {
        private readonly ICategoryInterface _categoryInterface;

        public CategoryController(ICategoryInterface categoryInterface)
        {
            _categoryInterface = categoryInterface;
        }

        [Route("")]
        [Route("Index")]
        public IActionResult Index()
        {
            return View("~/Views/Admin/Category/Index.cshtml");
        }

        [HttpPost]
        [Route("GetCategoryList")]
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
        public async Task<IActionResult> Create([FromBody] CategoryUpdateInputVIEW viewInput)
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
                var input = new CategoryUpdateInput
                {
                    UserAction = 1, // 1 = Add
                    CategoryID = viewInput.CategoryID,
                    CategoryName = viewInput.CategoryName,
                    Description = viewInput.Description,
                    IsActive = viewInput.IsActive ?? true,
                    EnterBy = 1 // TODO: Get from session/auth
                };

                var result = await _categoryInterface.CreateCategoryAsync(input);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IActionResult> Update([FromBody] CategoryUpdateInputVIEW viewInput)
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
                var input = new CategoryUpdateInput
                {
                    UserAction = 2, // 2 = Edit
                    CategoryID = viewInput.CategoryID,
                    CategoryName = viewInput.CategoryName,
                    Description = viewInput.Description,
                    IsActive = viewInput.IsActive ?? true,
                    EnterBy = 1 // TODO: Get from session/auth
                };

                var result = await _categoryInterface.UpdateCategoryAsync(input);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost]
        [Route("Delete")]
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
