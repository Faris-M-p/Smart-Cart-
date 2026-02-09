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
        private object _logger;

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
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Unhandled error");

                //----------------------------------
                // SERVER FAILURE
                //----------------------------------
                return StatusCode(500, new ApiResponse<TableOutput<Category>>
                {
                    Success = false,
                    Message = "Internal server error"
                });
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
                    EnterBy = 1 // TODO: Get from session/auth
                };

                var result = await _categoryInterface.CreateCategoryAsync(input);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new CommonResponse
                {
                    ResponseCode = -1,
                    StatusCode = false,
                    ResponseMsg = $"An error occurred: {ex.Message}"
                });
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
                    EnterBy = 1 // TODO: Get from session/auth
                };

                var result = await _categoryInterface.UpdateCategoryAsync(input);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new CommonResponse
                {
                    ResponseCode = -1,
                    StatusCode = false,
                    ResponseMsg = $"An error occurred: {ex.Message}"
                });
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
            catch (Exception ex)
            {
                return StatusCode(500, new CommonResponse
                {
                    ResponseCode = -1,
                    StatusCode = false,
                    ResponseMsg = $"An error occurred: {ex.Message}"
                });
            }
        }

        [HttpGet]
        [Route("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var input = new CategoryListInput
                {
                    FilterCategoryIDs = $"[{{\"ID_Value\":{id}}}]",
                    PageIndex = 1,
                    PageSize = 1
                };

                var result = await _categoryInterface.GetCategoryListAsync(input);
                
                if (result?.TableData != null && result.TableData.Any())
                {
                    return Ok(result.TableData.First());
                }

                return NotFound(new { message = "Category not found." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }
       


    }
}
