using Microsoft.AspNetCore.Mvc;
using Ecommerce.Interface.Admin;
using static Ecommerce.Models.Admin.BrandModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Controllers.Admin
{
    [Route("Admin/Brand")]
    public class BrandController : Controller
    {
        private readonly IBrandInterface _brandInterface;

        public BrandController(IBrandInterface brandInterface)
        {
            _brandInterface = brandInterface;
        }

        [Route("")]
        [Route("Index")]
        public IActionResult Index()
        {
            return View("~/Views/Admin/Brand/Index.cshtml");
        }

        [HttpPost]
        [Route("GetBrandList")]
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
                return StatusCode(500, new ApiResponse<TableOutput<Brand>>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> Create([FromBody] BrandUpdateInputVIEW viewInput)
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
        public async Task<IActionResult> Update([FromBody] BrandUpdateInputVIEW viewInput)
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
                var row = await _brandInterface.GetBrandByIdAsync(id);
                if (row == null)
                {
                    return NotFound(new { message = "Brand not found." });
                }

                return Ok(row);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }
    }
}
