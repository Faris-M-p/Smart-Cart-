using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Ecommerce.CustomModelValidation;

namespace Ecommerce.Models.Admin
{
    public class UserRoleModel
    {
        public class UserRoleListInputVIEW
        {
            [Display(Name = "Search Text")]
            public string SearchText { get; set; } = string.Empty;

            [Display(Name = "Filter User Role IDs")]
            public string FilterUserRoleIDs { get; set; } = string.Empty;

            [Display(Name = "Page Index")]
            [GreaterThanZero]
            public int PageIndex { get; set; } = 1;

            [Display(Name = "Page Size")]
            [GreaterThanZero]
            [Range(1, 100, ErrorMessage = "{0} must be between 1 and 100.")]
            public int PageSize { get; set; } = 10;

            [Display(Name = "Sort Column")]
            [Range(0, 3, ErrorMessage = "{0} must be between {1} and {2}.")]
            public int SortColumn { get; set; }

            [Display(Name = "Sort Mode")]
            public string SortMode { get; set; } = string.Empty;
        }

        public class UserRoleUpdateInputVIEW
        {
            [Display(Name = "User Role ID")]
            public int UserRoleID { get; set; } = 0;

            [Display(Name = "Role Name")]
            [Required(ErrorMessage = "{0} is required.")]
            [RequiredNotEmpty]
            [MaxLength(100, ErrorMessage = "{0} cannot exceed 100 characters.")]
            public string RoleName { get; set; } = string.Empty;

            [Display(Name = "Description")]
            [MaxLength(1000, ErrorMessage = "{0} cannot exceed 1000 characters.")]
            public string Description { get; set; } = string.Empty;

            [Display(Name = "Active")]
            public bool? IsActive { get; set; }
        }

        public class UserRolePermissionSaveInputVIEW
        {
            [Display(Name = "User Role ID")]
            [Required(ErrorMessage = "{0} is required.")]
            [GreaterThanZero]
            [JsonPropertyName("id_UserRole")]
            public int UserRoleID { get; set; }

            [JsonPropertyName("FK_UserRole")]
            public int FK_UserRole
            {
                get => UserRoleID;
                set { if (value > 0) UserRoleID = value; }
            }

            [Display(Name = "Selected Permissions")]
            [NotEmptyList(ErrorMessage = "At least one permission must be selected.")]
            [JsonPropertyName("selectedPermissionIds")]
            public List<int> SelectedPermissionIds { get; set; } = new();

            [JsonPropertyName("PermissionIds")]
            public List<int>? PermissionIds
            {
                get => SelectedPermissionIds;
                set { if (value != null && value.Count > 0) SelectedPermissionIds = value; }
            }
        }

        public class UserRoleDeleteInputVIEW
        {
            [Display(Name = "User Role ID")]
            [Required(ErrorMessage = "{0} is required.")]
            [GreaterThanZero]
            public int UserRoleID { get; set; }

            [Display(Name = "Cancelled Reason")]
            [MaxLength(255, ErrorMessage = "{0} cannot exceed 255 characters.")]
            public string CancelledReason { get; set; } = string.Empty;
        }

        public class UserRoleListInput
        {
            [Display(Name = "Search Text")]
            public string SearchText { get; set; } = string.Empty;

            [Display(Name = "Filter User Role IDs")]
            public string FilterUserRoleIDs { get; set; } = string.Empty;

            [Display(Name = "Page Index")]
            public int PageIndex { get; set; } = 1;

            [Display(Name = "Page Size")]
            public int PageSize { get; set; } = 10;

            [Display(Name = "Sort Column")]
            public int SortColumn { get; set; }

            [Display(Name = "Sort Mode")]
            public string SortMode { get; set; } = string.Empty;
        }

        public class UserRoleUpdateInput
        {
            [Display(Name = "User Action")]
            public int UserAction { get; set; }

            [Display(Name = "User Role ID")]
            public int UserRoleID { get; set; } = 0;

            [Display(Name = "Role Name")]
            public string RoleName { get; set; } = string.Empty;

            [Display(Name = "Description")]
            public string Description { get; set; } = string.Empty;

            [Display(Name = "Is Active")]
            public bool IsActive { get; set; } = true;

            [Display(Name = "Enter By")]
            public int EnterBy { get; set; } = 1;
        }

        public class UserRolePermissionSaveInput
        {
            [Display(Name = "User Role ID")]
            public int UserRoleID { get; set; }

            [Display(Name = "Selected Permissions")]
            public List<int> SelectedPermissionIds { get; set; } = new();
        }

        public class UserRoleDeleteInput
        {
            [Display(Name = "User Role ID")]
            public int UserRoleID { get; set; }

            [Display(Name = "Cancelled Reason")]
            public string CancelledReason { get; set; } = string.Empty;

            [Display(Name = "Enter By")]
            public int EnterBy { get; set; } = 1;
        }

        public class UserRole
        {
            [Display(Name = "User Role ID")]
            public int UserRoleID { get; set; }

            [Display(Name = "Role Name")]
            public string RoleName { get; set; } = string.Empty;

            [Display(Name = "Description")]
            public string? Description { get; set; }

            [Display(Name = "System Role")]
            public bool IsSystemRole { get; set; }

            [Display(Name = "Is Active")]
            public bool IsActive { get; set; }

            [Display(Name = "Cancelled")]
            public bool Cancelled { get; set; }

            [Display(Name = "Cancelled On")]
            public DateTime? CancelledOn { get; set; }

            [Display(Name = "Cancelled Reason")]
            public string? CancelledReason { get; set; }

            [Display(Name = "Selected Permissions")]
            public List<int> SelectedPermissionIds { get; set; } = new();
        }

        public class PermissionNode
        {
            public int PermissionID { get; set; }
            public string PermissionName { get; set; } = string.Empty;
            public string PermissionCode { get; set; } = string.Empty;
        }

        public class ModulePermissionNode
        {
            public int ModuleID { get; set; }
            public string ModuleName { get; set; } = string.Empty;
            public string DisplayName { get; set; } = string.Empty;
            public List<PermissionNode> Permissions { get; set; } = new();
        }

        public class PermissionGroupNode
        {
            public string GroupKey { get; set; } = string.Empty;
            public string GroupName { get; set; } = string.Empty;
            public List<ModulePermissionNode> Features { get; set; } = new();
        }
    }
}
