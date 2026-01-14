using Microsoft.AspNetCore.Mvc;
using Ecommerce.Interface.Admin;
using static Ecommerce.Models.Admin.SupplierModel;
using static Ecommerce.Models.CommonModel;
using System.Linq;

namespace Ecommerce.Controllers.Admin
{
    [Route("Admin/Supplier")]
    public class SupplierController : Controller
    {
        private readonly ISupplierInterface _supplierInterface;

        public SupplierController(ISupplierInterface supplierInterface)
        {
            _supplierInterface = supplierInterface;
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
                    return BadRequest(new { message = "Validation failed.", errors });
                }

                // Map VIEW model to Procedure Input model
                var input = new SupplierListInput
                {
                    SearchText = viewInput.SearchText,
                    FilterSupplierIDs = viewInput.FilterSupplierIDs,
                    ShowCancelled = viewInput.ShowCancelled,
                    PageIndex = viewInput.PageIndex,
                    PageSize = viewInput.PageSize,
                    SortColumn = viewInput.SortColumn,
                    SortMode = viewInput.SortMode
                };

                var result = await _supplierInterface.GetSupplierListAsync(input);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
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

                // Map VIEW model to Procedure Input model
                var input = new SupplierUpdateInput
                {
                    UserAction = 1, // 1 = Add
                    ID_Supplier = viewInput.ID_Supplier,
                    SupplierName = viewInput.SupplierName,
                    ContactPerson = viewInput.ContactPerson,
                    Phone = viewInput.Phone,
                    Email = viewInput.Email,
                    GSTNumber = viewInput.GSTNumber,
                    Address = viewInput.Address,
                    EnterBy = 1 // TODO: Get from session/auth
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

                // Map VIEW model to Procedure Input model
                var input = new SupplierUpdateInput
                {
                    UserAction = 2, // 2 = Edit
                    ID_Supplier = viewInput.ID_Supplier,
                    SupplierName = viewInput.SupplierName,
                    ContactPerson = viewInput.ContactPerson,
                    Phone = viewInput.Phone,
                    Email = viewInput.Email,
                    GSTNumber = viewInput.GSTNumber,
                    Address = viewInput.Address,
                    EnterBy = 1 // TODO: Get from session/auth
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

                // Map VIEW model to Procedure Input model
                // Note: ProSupplierUpdate uses UserAction = 3 for Delete
                var input = new SupplierUpdateInput
                {
                    UserAction = 3, // 3 = Delete (soft delete)
                    ID_Supplier = viewInput.ID_Supplier,
                    SupplierName = string.Empty, // Not needed for delete
                    CancelledReason = viewInput.CancelledReason,
                    EnterBy = 1 // TODO: Get from session/auth
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
        public async Task<IActionResult> GetById(long id)
        {
            try
            {
                var supplier = await _supplierInterface.GetSupplierByIdAsync(id);
                
                if (supplier != null)
                {
                    return Ok(supplier);
                }

                return NotFound(new { message = "Supplier not found." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }
    }
}
