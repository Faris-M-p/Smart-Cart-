using System.ComponentModel.DataAnnotations;
using Ecommerce.CustomModelValidation;

namespace Ecommerce.Models.Admin
{
    public class SubCategoryModel
    {
        // VIEW Models - For JavaScript/Frontend Input
        public class SubCategoryListInputVIEW
        {
            [Display(Name = "Search Text")]
            public string SearchText { get; set; } = string.Empty;

            [Display(Name = "Filter Category IDs")]
            public string FilterCategoryIDs { get; set; } = string.Empty;

            [Display(Name = "Filter SubCategory IDs")]
            public string FilterSubCategoryIDs { get; set; } = string.Empty;

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
            public string SortMode { get; set; } = string.Empty; // ASC / DESC
        }

        public class SubCategoryUpdateInputVIEW
        {
            [Display(Name = "SubCategory ID")]
            public int SubCategoryID { get; set; } = 0;

            [Display(Name = "SubCategory Name")]
            [Required(ErrorMessage = "{0} is required.")]
            [RequiredNotEmpty]
            [System.ComponentModel.DataAnnotations.MaxLength(255, ErrorMessage = "{0} cannot exceed 255 characters.")]
            public string SubCategoryName { get; set; } = string.Empty;

            [Display(Name = "Category")]
            [Required(ErrorMessage = "{0} is required.")]
            [GreaterThanZero(ErrorMessage = "{0} must be selected.")]
            public int FK_Category { get; set; }

            [Display(Name = "Description")]
            [System.ComponentModel.DataAnnotations.MaxLength(500, ErrorMessage = "{0} cannot exceed 500 characters.")]
            public string Description { get; set; } = string.Empty;
        }

        public class SubCategoryDeleteInputVIEW
        {
            [Display(Name = "SubCategory ID")]
            [Required(ErrorMessage = "{0} is required.")]
            [GreaterThanZero]
            public int SubCategoryID { get; set; }

            [Display(Name = "Cancelled Reason")]
            [System.ComponentModel.DataAnnotations.MaxLength(500, ErrorMessage = "{0} cannot exceed 500 characters.")]
            public string CancelledReason { get; set; } = string.Empty;
        }

        // Procedure Input Models - For Stored Procedures
        public class SubCategoryListInput
        {
            [Display(Name = "Search Text")]
            public string SearchText { get; set; } = string.Empty;

            [Display(Name = "Filter Category IDs")]
            public string FilterCategoryIDs { get; set; } = string.Empty;

            [Display(Name = "Filter SubCategory IDs")]
            public string FilterSubCategoryIDs { get; set; } = string.Empty;

            [Display(Name = "Page Index")]
            public int PageIndex { get; set; } = 1;

            [Display(Name = "Page Size")]
            public int PageSize { get; set; } = 10;

            [Display(Name = "Sort Column")]
            public int SortColumn { get; set; }

            [Display(Name = "Sort Mode")]
            public string SortMode { get; set; } = string.Empty; // ASC / DESC
        }

        public class SubCategoryUpdateInput
        {
            [Display(Name = "User Action")]
            public int UserAction { get; set; } // 1 = Add, 2 = Edit

            [Display(Name = "SubCategory ID")]
            public int SubCategoryID { get; set; } = 0;

            [Display(Name = "SubCategory Name")]
            public string SubCategoryName { get; set; } = string.Empty;

            [Display(Name = "Category")]
            public int FK_Category { get; set; }

            [Display(Name = "Description")]
            public string Description { get; set; } = string.Empty;

            [Display(Name = "Enter By")]
            public int EnterBy { get; set; } = 1; // TODO: Get from session/auth
        }

        public class SubCategoryDeleteInput
        {
            [Display(Name = "SubCategory ID")]
            public int SubCategoryID { get; set; }

            [Display(Name = "Cancelled Reason")]
            public string CancelledReason { get; set; } = string.Empty;

            [Display(Name = "Enter By")]
            public int EnterBy { get; set; } = 1; // TODO: Get from session/auth
        }

        // Output Models from Stored Procedures
        public class SubCategory
        {
            [Display(Name = "SubCategory ID")]
            public int ID_SubCategory { get; set; }

            [Display(Name = "SubCategory Name")]
            public string SubCategoryName { get; set; } = string.Empty;

            [Display(Name = "Category ID")]
            public int FK_Category { get; set; }

            [Display(Name = "Description")]
            public string? Description { get; set; }

            [Display(Name = "Created Date")]
            public DateTime? CreatedDate { get; set; }

            [Display(Name = "Cancelled")]
            public bool Cancelled { get; set; }

            [Display(Name = "Cancelled On")]
            public DateTime? CancelledOn { get; set; }

            [Display(Name = "Cancelled Reason")]
            public string? CancelledReason { get; set; }
        }
    }
}
