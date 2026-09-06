using Microsoft.AspNetCore.Mvc;
using Ecommerce.Filters;
using Ecommerce.Interface.Admin;
using static Ecommerce.Models.Admin.EmployeeModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Controllers.Admin
{
    [Route("Admin/Employee")]
    public class EmployeeController : Controller
    {
        private readonly IEmployeeInterface _employeeInterface;

        public EmployeeController(IEmployeeInterface employeeInterface)
        {
            _employeeInterface = employeeInterface;
        }

        [HttpGet("/admin/employees")]
        [Route("")]
        [Route("Index")]
        [RequirePermission("Employees.View")]
        public IActionResult Index()
        {
            return View("~/Views/Admin/Employee/Index.cshtml");
        }

        [HttpPost]
        [Route("GetEmployeeList")]
        [RequirePermission("Employees.View")]
        public async Task<IActionResult> GetEmployeeList([FromBody] EmployeeListInputVIEW viewInput)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new ApiResponse<TableOutput<Employee>>
                    {
                        Success = false,
                        Message = "Validation failed",
                        Errors = errors
                    });
                }

                var input = new EmployeeListInput
                {
                    SearchText = viewInput.SearchText,
                    PageIndex = viewInput.PageIndex,
                    PageSize = viewInput.PageSize,
                    SortColumn = viewInput.SortColumn,
                    SortMode = viewInput.SortMode
                };

                var result = await _employeeInterface.GetEmployeeListAsync(input);
                return Ok(new ApiResponse<TableOutput<Employee>>
                {
                    Success = true,
                    Message = "Employees loaded successfully",
                    Data = result
                });
            }
            catch
            {
                throw;
            }
        }

        [HttpGet]
        [Route("GetById/{id}")]
        [RequirePermission("Employees.View")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var row = await _employeeInterface.GetEmployeeByIdAsync(id);
                if (row == null)
                {
                    return NotFound(new { message = "Employee not found." });
                }

                return Ok(row);
            }
            catch
            {
                throw;
            }
        }

        [HttpGet]
        [Route("GetActiveUserRoles")]
        [RequirePermission("Employees.View")]
        public async Task<IActionResult> GetActiveUserRoles()
        {
            try
            {
                var rows = await _employeeInterface.GetActiveUserRolesAsync();
                return Ok(rows);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost]
        [Route("Create")]
        [RequirePermission("Employees.Create")]
        public async Task<IActionResult> Create([FromBody] EmployeeUpdateInputVIEW viewInput)
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

                var result = await _employeeInterface.CreateEmployeeAsync(MapWriteInput(viewInput));
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost]
        [Route("Update")]
        [RequirePermission("Employees.Edit")]
        public async Task<IActionResult> Update([FromBody] EmployeeUpdateInputVIEW viewInput)
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

                var result = await _employeeInterface.UpdateEmployeeAsync(MapWriteInput(viewInput));
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost]
        [Route("Delete")]
        [RequirePermission("Employees.Deactivate")]
        public async Task<IActionResult> Delete([FromBody] EmployeeDeleteInputVIEW viewInput)
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

                var input = new EmployeeDeleteInput
                {
                    EmployeeID = viewInput.EmployeeID,
                    CancelledReason = viewInput.CancelledReason
                };

                var result = await _employeeInterface.DeleteEmployeeAsync(input);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        private static EmployeeUpdateInput MapWriteInput(EmployeeUpdateInputVIEW viewInput) =>
            new()
            {
                EmployeeID = viewInput.EmployeeID,
                EmployeeName = viewInput.EmployeeName,
                UserName = viewInput.UserName,
                Password = viewInput.Password,
                ConfirmPassword = viewInput.ConfirmPassword,
                FK_UserRole = viewInput.FK_UserRole,
                IsActive = viewInput.IsActive ?? true
            };
    }
}
