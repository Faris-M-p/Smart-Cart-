using Microsoft.AspNetCore.Mvc;
using Ecommerce.Interface.Admin;
using static Ecommerce.Models.Admin.SupplierModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Controllers.Admin
{
    [Route("Admin/Supplier")]
    public class SupplierController : Controller
    {
        private readonly ISupplierInterface _supplierInterface;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public SupplierController(
            ISupplierInterface supplierInterface,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _supplierInterface = supplierInterface;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [Route("")]
        [Route("Index")]
        public IActionResult Index()
        {
            return View("~/Views/Admin/Supplier/Index.cshtml");
        }

        [HttpPost]
        [Route("GetSupplierList")]
        public async Task<IActionResult> GetSupplierList([FromBody] SupplierListInputVIEW viewInput)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new ApiResponse<TableOutput<Supplier>>
                    {
                        Success = false,
                        Message = "Validation failed",
                        Errors = errors
                    });
                }

                var input = new SupplierListInput
                {
                    SearchText = viewInput.SearchText,
                    FilterSupplierIDs = viewInput.FilterSupplierIDs,
                    PageIndex = viewInput.PageIndex,
                    PageSize = viewInput.PageSize,
                    SortColumn = viewInput.SortColumn,
                    SortMode = viewInput.SortMode
                };

                var result = await _supplierInterface.GetSupplierListAsync(input);

                return Ok(new ApiResponse<TableOutput<Supplier>>
                {
                    Success = true,
                    Message = "Suppliers loaded successfully",
                    Data = result
                });
            }
            catch
            {
                return StatusCode(500, new ApiResponse<TableOutput<Supplier>>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> Create([FromBody] SupplierUpdateInputVIEW viewInput)
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

                var input = new SupplierUpdateInput
                {
                    UserAction = 1,
                    SupplierId = viewInput.SupplierId,
                    SupplierName = viewInput.SupplierName,
                    CompanyName = viewInput.CompanyName,
                    Phone = viewInput.Phone,
                    Email = viewInput.Email,
                    State = viewInput.State,
                    District = viewInput.District,
                    City = viewInput.City,
                    Address = viewInput.Address,
                    Pincode = viewInput.Pincode,
                    Description = viewInput.Description,
                    IsActive = viewInput.IsActive ?? true,
                    EnterBy = 1
                };

                var result = await _supplierInterface.CreateSupplierAsync(input);
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
        public async Task<IActionResult> Update([FromBody] SupplierUpdateInputVIEW viewInput)
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

                var input = new SupplierUpdateInput
                {
                    UserAction = 2,
                    SupplierId = viewInput.SupplierId,
                    SupplierName = viewInput.SupplierName,
                    CompanyName = viewInput.CompanyName,
                    Phone = viewInput.Phone,
                    Email = viewInput.Email,
                    State = viewInput.State,
                    District = viewInput.District,
                    City = viewInput.City,
                    Address = viewInput.Address,
                    Pincode = viewInput.Pincode,
                    Description = viewInput.Description,
                    IsActive = viewInput.IsActive ?? true,
                    EnterBy = 1
                };

                var result = await _supplierInterface.UpdateSupplierAsync(input);
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
        public async Task<IActionResult> Delete([FromBody] SupplierDeleteInputVIEW viewInput)
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

                var input = new SupplierDeleteInput
                {
                    SupplierId = viewInput.SupplierId,
                    CancelledReason = viewInput.CancelledReason,
                    EnterBy = 1
                };

                var result = await _supplierInterface.DeleteSupplierAsync(input);
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
                var row = await _supplierInterface.GetSupplierByIdAsync(id);
                if (row == null)
                {
                    return NotFound(new { message = "Supplier not found." });
                }

                return Ok(row);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }

        /// <summary>Proxy to CountryStateCity API (India states). Requires <c>CountryStateCity:ApiKey</c>.</summary>
        [HttpGet]
        [Route("Location/IndiaStates")]
        public async Task<IActionResult> GetIndiaStates(CancellationToken cancellationToken)
        {
            var apiKey = _configuration["CountryStateCity:ApiKey"] ?? string.Empty;
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return Content("[]", "application/json");
            }

            var client = _httpClientFactory.CreateClient("CountryStateCity");
            using var request = new HttpRequestMessage(HttpMethod.Get, "countries/IN/states");
            request.Headers.TryAddWithoutValidation("X-CSCAPI-KEY", apiKey.Trim());

            using var response = await client.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            return new ContentResult
            {
                Content = body,
                ContentType = "application/json",
                StatusCode = (int)response.StatusCode
            };
        }

        /// <summary>Proxy to CountryStateCity API (cities in an Indian state). District dropdown uses this list; city narrows to the selected district name.</summary>
        [HttpGet]
        [Route("Location/IndiaCities/{stateIso}")]
        public async Task<IActionResult> GetIndiaCitiesForState(string stateIso, CancellationToken cancellationToken)
        {
            var apiKey = _configuration["CountryStateCity:ApiKey"] ?? string.Empty;
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return Content("[]", "application/json");
            }

            var code = (stateIso ?? string.Empty).Trim().ToUpperInvariant();
            if (code.Length == 0 || code.Length > 10)
            {
                return Content("[]", "application/json");
            }

            var client = _httpClientFactory.CreateClient("CountryStateCity");
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"countries/IN/states/{Uri.EscapeDataString(code)}/cities");
            request.Headers.TryAddWithoutValidation("X-CSCAPI-KEY", apiKey.Trim());

            using var response = await client.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
           
            return new ContentResult
            {
                Content = body,
                ContentType = "application/json",
                StatusCode = (int)response.StatusCode
            };
        }
    }
}
