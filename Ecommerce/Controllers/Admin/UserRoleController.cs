using Microsoft.AspNetCore.Mvc;
using Ecommerce.Filters;
using Ecommerce.Interface.Admin;
using static Ecommerce.Models.Admin.UserRoleModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Controllers.Admin
{
    [Route("Admin/UserRole")]
    public class UserRoleController : Controller
    {
        private readonly IUserRoleInterface _userRoleInterface;

        public UserRoleController(IUserRoleInterface userRoleInterface)
        {
            _userRoleInterface = userRoleInterface;
        }

        [Route("")]
        [Route("Index")]
        [RequirePermission("UserRoles.View")]
        public IActionResult Index()
        {
            return View("~/Views/Admin/UserRole/Index.cshtml");
        }

        [HttpPost]
        [Route("GetUserRoleList")]
        [RequirePermission("UserRoles.View")]
        public async Task<IActionResult> GetUserRoleList(
            [FromBody] UserRoleListInputVIEW viewInput)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new ApiResponse<TableOutput<UserRole>>
                    {
                        Success = false,
                        Message = "Validation failed",
                        Errors = errors
                    });
                }

                var input = new UserRoleListInput
                {
                    SearchText = viewInput.SearchText,
                    FilterUserRoleIDs = viewInput.FilterUserRoleIDs,
                    PageIndex = viewInput.PageIndex,
                    PageSize = viewInput.PageSize,
                    SortColumn = viewInput.SortColumn,
                    SortMode = viewInput.SortMode
                };

                var result = await _userRoleInterface
                    .GetUserRoleListAsync(input);

                return Ok(new ApiResponse<TableOutput<UserRole>>
                {
                    Success = true,
                    Message = "User roles loaded successfully",
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
        [RequirePermission("UserRoles.Create")]
        public async Task<IActionResult> Create([FromBody] UserRoleUpdateInputVIEW viewInput)
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

                var input = new UserRoleUpdateInput
                {
                    UserAction = 1,
                    UserRoleID = viewInput.UserRoleID,
                    RoleName = viewInput.RoleName,
                    Description = viewInput.Description,
                    IsActive = viewInput.IsActive ?? true,
                    EnterBy = 1
                };

                var result = await _userRoleInterface.CreateUserRoleAsync(input);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost]
        [Route("Update")]
        [RequirePermission("UserRoles.Edit")]
        public async Task<IActionResult> Update([FromBody] UserRoleUpdateInputVIEW viewInput)
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

                var input = new UserRoleUpdateInput
                {
                    UserAction = 2,
                    UserRoleID = viewInput.UserRoleID,
                    RoleName = viewInput.RoleName,
                    Description = viewInput.Description,
                    IsActive = viewInput.IsActive ?? true,
                    EnterBy = 1
                };

                var result = await _userRoleInterface.UpdateUserRoleAsync(input);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost]
        [Route("Delete")]
        [RequirePermission("UserRoles.Delete")]
        public async Task<IActionResult> Delete([FromBody] UserRoleDeleteInputVIEW viewInput)
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

                var input = new UserRoleDeleteInput
                {
                    UserRoleID = viewInput.UserRoleID,
                    CancelledReason = viewInput.CancelledReason,
                    EnterBy = 1
                };

                var result = await _userRoleInterface.DeleteUserRoleAsync(input);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        [HttpGet]
        [Route("GetById/{id}")]
        [RequirePermission("UserRoles.View")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var row = await _userRoleInterface.GetUserRoleByIdAsync(id);
                if (row == null)
                {
                    return NotFound(new { message = "User role not found." });
                }

                return Ok(row);
            }
            catch
            {
                throw;
            }
        }

        [HttpGet("/admin/user-roles/{id:int}/permissions")]
        [RequirePermission("UserRoles.Edit")]
        public async Task<IActionResult> PermissionMapping(int id)
        {
            try
            {
                var role = await _userRoleInterface.GetUserRoleByIdAsync(id);
                if (role == null || role.Cancelled)
                {
                    ViewBag.IsBlocked = true;
                    ViewBag.BlockedMessage = "User role not found.";
                    return View("~/Views/Admin/UserRole/Permissions.cshtml", role);
                }

                if (role.IsSystemRole)
                {
                    ViewBag.IsBlocked = true;
                    ViewBag.BlockedMessage = "This system role cannot be modified.";
                    return View("~/Views/Admin/UserRole/Permissions.cshtml", role);
                }

                ViewBag.IsBlocked = false;
                return View("~/Views/Admin/UserRole/Permissions.cshtml", role);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost]
        [Route("SavePermissions")]
        [RequirePermission("UserRoles.Edit")]
        public async Task<IActionResult> SavePermissions([FromBody] UserRolePermissionSaveInputVIEW viewInput)
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

                var input = new UserRolePermissionSaveInput
                {
                    UserRoleID = viewInput.UserRoleID,
                    SelectedPermissionIds = viewInput.SelectedPermissionIds
                };

                var result = await _userRoleInterface.SaveUserRolePermissionsAsync(input);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        [HttpGet]
        [Route("GetPermissionTree")]
        [RequirePermission("UserRoles.Edit")]
        public async Task<IActionResult> GetPermissionTree()
        {
            try
            {
                var rows = await _userRoleInterface.GetPermissionTreeAsync();
                return Ok(rows);
            }
            catch
            {
                throw;
            }
        }
    }
}
