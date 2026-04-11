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
        [Route("GetVariantValueList")]
        public async Task<IActionResult> GetVariantValueList([FromBody] VariantValueListInputVIEW viewInput)
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
                var input = new VariantValueListInput
                {
                    SearchText = viewInput.SearchText,
                    FilterVariantIDs = viewInput.FilterVariantIDs,
                    PageIndex = viewInput.PageIndex,
                    PageSize = viewInput.PageSize,
                    SortColumn = viewInput.SortColumn,
                    SortMode = viewInput.SortMode
                };

                var result = await _variantValueInterface.GetVariantValueListAsync(input);
                return Ok(result);
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

                // Map VIEW model to Procedure Input model
                var input = new VariantValueUpdateInput
                {
                    UserAction = 1, // 1 = Insert
                    ID_VariantValue = viewInput.ID_VariantValue,
                    FK_Variant = viewInput.FK_Variant,
                    ValueName = viewInput.ValueName,
                    Description = viewInput.Description,
                    ValueIcon = viewInput.ValueIcon,
                    DisplayOrder = viewInput.DisplayOrder,
                    EnterBy = 1 // TODO: Get from session/auth
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

                // Map VIEW model to Procedure Input model
                var input = new VariantValueUpdateInput
                {
                    UserAction = 2, // 2 = Update
                    ID_VariantValue = viewInput.ID_VariantValue,
                    FK_Variant = viewInput.FK_Variant,
                    ValueName = viewInput.ValueName,
                    Description = viewInput.Description,
                    ValueIcon = viewInput.ValueIcon,
                    DisplayOrder = viewInput.DisplayOrder,
                    EnterBy = 1 // TODO: Get from session/auth
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

                // Map VIEW model to Procedure Input model
                var input = new VariantValueUpdateInput
                {
                    UserAction = 3, // 3 = Delete
                    ID_VariantValue = viewInput.ID_VariantValue,
                    CancelledReason = viewInput.CancelledReason,
                    EnterBy = 1 // TODO: Get from session/auth
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
                    PageSize = 1000, // Get all variants
                    SearchText = string.Empty,
                    FilterVariantIDs = string.Empty,
                    SortColumn = (int)VariantSortColumn.Name,
                    SortMode = "ASC"
                };

                var result = await _variantInterface.GetVariantListAsync(input);
                
                if (result != null && result.TableData != null)
                {
                    var variants = result.TableData
                        .Where(v => !v.Cancelled)
                        .Select(v => new { v.ID_Variant, v.VariantName })
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
