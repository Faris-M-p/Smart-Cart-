using Microsoft.AspNetCore.Mvc;
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

        public SubCategoryController(ISubCategoryInterface subCategoryInterface, ICategoryInterface categoryInterface)
        {
            _subCategoryInterface = subCategoryInterface;
            _categoryInterface = categoryInterface;
        }

        [Route("")]
        [Route("Index")]
        public IActionResult Index()
        {
            return View("~/Views/Admin/SubCategory/Index.cshtml");
        }

        [HttpPost]
        [Route("GetSubCategoryList")]
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
        public async Task<IActionResult> Create([FromBody] SubCategoryUpdateInputVIEW viewInput)
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
                var input = new SubCategoryUpdateInput
                {
                    UserAction = 1, // 1 = Add
                    SubCategoryID = viewInput.SubCategoryID,
                    SubCategoryName = viewInput.SubCategoryName,
                    FK_Category = viewInput.FK_Category,
                    Description = viewInput.Description,
                    IsActive = viewInput.IsActive ?? true,
                    EnterBy = 1 // TODO: Get from session/auth
                };

                var result = await _subCategoryInterface.CreateSubCategoryAsync(input);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IActionResult> Update([FromBody] SubCategoryUpdateInputVIEW viewInput)
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
                var input = new SubCategoryUpdateInput
                {
                    UserAction = 2, // 2 = Edit
                    SubCategoryID = viewInput.SubCategoryID,
                    SubCategoryName = viewInput.SubCategoryName,
                    FK_Category = viewInput.FK_Category,
                    Description = viewInput.Description,
                    IsActive = viewInput.IsActive ?? true,
                    EnterBy = 1 // TODO: Get from session/auth
                };

                var result = await _subCategoryInterface.UpdateSubCategoryAsync(input);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost]
        [Route("Delete")]
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
