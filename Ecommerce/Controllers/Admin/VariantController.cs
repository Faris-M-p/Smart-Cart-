using Microsoft.AspNetCore.Mvc;
using Ecommerce.Filters;
using Ecommerce.Interface.Admin;
using static Ecommerce.Models.Admin.VariantModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Controllers.Admin
{
    [Route("Admin/Variant")]
    public class VariantController : Controller
    {
        private readonly IVariantInterface _variantInterface;

        public VariantController(IVariantInterface variantInterface)
        {
            _variantInterface = variantInterface;
        }

        [Route("")]
        [Route("Index")]
        [RequirePermission("Variants.View")]
        public IActionResult Index()
        {
            return View("~/Views/Admin/Variant/Index.cshtml");
        }

        [HttpPost]
        [Route("GetVariantList")]
        [RequirePermission("Variants.View")]
        public async Task<IActionResult> GetVariantList([FromBody] VariantListInputVIEW viewInput)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new ApiResponse<TableOutput<Variant>>
                    {
                        Success = false,
                        Message = "Validation failed",
                        Errors = errors
                    });
                }

                var input = new VariantListInput
                {
                    SearchText = viewInput.SearchText,
                    FilterVariantIDs = viewInput.FilterVariantIDs,
                    PageIndex = viewInput.PageIndex,
                    PageSize = viewInput.PageSize,
                    SortColumn = viewInput.SortColumn,
                    SortMode = viewInput.SortMode
                };

                var result = await _variantInterface.GetVariantListAsync(input);

                return Ok(new ApiResponse<TableOutput<Variant>>
                {
                    Success = true,
                    Message = "Variants loaded successfully",
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
        [RequirePermission("Variants.Create")]
        public async Task<IActionResult> Create([FromBody] VariantUpdateInputVIEW viewInput)
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

                var input = new VariantUpdateInput
                {
                    VariantID = viewInput.VariantID,
                    Name = viewInput.Name,
                    Description = viewInput.Description,
                    DisplayOrder = viewInput.DisplayOrder,
                    IsActive = viewInput.IsActive ?? true,
                    EnterBy = 1
                };

                var result = await _variantInterface.CreateVariantAsync(input);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost]
        [Route("Update")]
        [RequirePermission("Variants.Edit")]
        public async Task<IActionResult> Update([FromBody] VariantUpdateInputVIEW viewInput)
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

                var input = new VariantUpdateInput
                {
                    VariantID = viewInput.VariantID,
                    Name = viewInput.Name,
                    Description = viewInput.Description,
                    DisplayOrder = viewInput.DisplayOrder,
                    IsActive = viewInput.IsActive ?? true,
                    EnterBy = 1
                };

                var result = await _variantInterface.UpdateVariantAsync(input);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost]
        [Route("Delete")]
        [RequirePermission("Variants.Delete")]
        public async Task<IActionResult> Delete([FromBody] VariantDeleteInputVIEW viewInput)
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

                var input = new VariantDeleteInput
                {
                    VariantID = viewInput.VariantID,
                    CancelledReason = viewInput.CancelledReason,
                    EnterBy = 1
                };

                var result = await _variantInterface.DeleteVariantAsync(input);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        [HttpGet]
        [Route("GetById/{id}")]
        [RequirePermission("Variants.View")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var row = await _variantInterface.GetVariantByIdAsync(id);
                if (row == null)
                {
                    return NotFound(new { message = "Variant not found." });
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
