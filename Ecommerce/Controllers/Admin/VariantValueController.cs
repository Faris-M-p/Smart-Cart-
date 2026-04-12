using Microsoft.AspNetCore.Mvc;
using Ecommerce.Interface.Admin;
using Ecommerce.Models.Enums;
using static Ecommerce.Models.Admin.VariantValueModel;
using static Ecommerce.Models.CommonModel;
using System.Linq;
using static Ecommerce.Models.Admin.VariantModel;

namespace Ecommerce.Controllers.Admin
{
    [Route("Admin/VariantValue")]
    public class VariantValueController : Controller
    {
        private readonly IVariantValueInterface _variantValueInterface;
        private readonly IVariantInterface _variantInterface;

        public VariantValueController(IVariantValueInterface variantValueInterface, IVariantInterface variantInterface)
        {
            _variantValueInterface = variantValueInterface;
            _variantInterface = variantInterface;
        }

        [Route("")]
        [Route("Index")]
        public IActionResult Index()
        {
            return View("~/Views/Admin/VariantValue/Index.cshtml");
        }

        [HttpPost]
        [Route("GetList")]
        [Route("GetVariantValueList")]
        public async Task<IActionResult> GetList([FromBody] VariantValueListInputVIEW viewInput)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new ApiResponse<TableOutput<VariantValue>>
                    {
                        Success = false,
                        Message = "Validation failed",
                        Errors = errors
                    });
                }

                var input = new VariantValueListInput
                {
                    FK_Variant = viewInput.FK_Variant,
                    SearchText = viewInput.SearchText,
                    FilterVariantIDs = viewInput.FilterVariantIDs,
                    PageIndex = viewInput.PageIndex,
                    PageSize = viewInput.PageSize,
                    SortColumn = viewInput.SortColumn,
                    SortMode = viewInput.SortMode
                };

                var result = await _variantValueInterface.GetVariantValueListAsync(input);

                return Ok(new ApiResponse<TableOutput<VariantValue>>
                {
                    Success = true,
                    Message = "Variant values loaded successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<TableOutput<VariantValue>>
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                });
            }
        }

        [HttpGet]
        [Route("GetByVariant/{variantId}")]
        public async Task<IActionResult> GetByVariant(int variantId)
        {
            try
            {
                var rows = await _variantValueInterface.GetByVariantIdAsync(variantId);
                return Ok(rows);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> Create([FromBody] VariantValueUpdateInputVIEW viewInput)
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

                var input = new VariantValueCreateInput
                {
                    FK_Variant = viewInput.FK_Variant,
                    Name = viewInput.Name,
                    Description = viewInput.Description,
                    DisplayOrder = viewInput.DisplayOrder,
                    EnterBy = 1
                };

                var result = await _variantValueInterface.CreateVariantValueAsync(input);
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
        public async Task<IActionResult> Update([FromBody] VariantValueUpdateInputVIEW viewInput)
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

                var input = new VariantValueUpdateInput
                {
                    VariantValueID = viewInput.VariantValueID,
                    Name = viewInput.Name,
                    Description = viewInput.Description,
                    DisplayOrder = viewInput.DisplayOrder,
                    EnterBy = 1
                };

                var result = await _variantValueInterface.UpdateVariantValueAsync(input);
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
        public async Task<IActionResult> Delete([FromBody] VariantValueDeleteInputVIEW viewInput)
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

                var input = new VariantValueDeleteInput
                {
                    VariantValueID = viewInput.VariantValueID,
                    EnterBy = 1
                };

                var result = await _variantValueInterface.DeleteVariantValueAsync(input);
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
                var result = await _variantValueInterface.GetVariantValueByIdAsync(id);

                if (result != null)
                {
                    return Ok(result);
                }

                return NotFound(new { message = "Variant Value not found." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet]
        [Route("GetVariants")]
        public async Task<IActionResult> GetVariants()
        {
            try
            {
                var input = new VariantListInput
                {
                    PageIndex = 1,
                    PageSize = 1000,
                    SearchText = string.Empty,
                    FilterVariantIDs = string.Empty,
                    SortColumn = (int)VariantSortColumn.Name,
                    SortMode = "ASC"
                };

                var result = await _variantInterface.GetVariantListAsync(input);

                if (result?.TableData != null)
                {
                    var variants = result.TableData
                        .Where(v => !v.Cancelled && v.IsActive)
                        .Select(v => new { ID_Variant = v.VariantID, VariantName = v.Name })
                        .ToList();
                    return Ok(variants);
                }

                return Ok(new List<object>());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }
    }
}
