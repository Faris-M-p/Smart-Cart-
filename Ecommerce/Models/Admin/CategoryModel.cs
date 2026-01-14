using System.ComponentModel.DataAnnotations;
using Ecommerce.CustomModelValidation;

namespace Ecommerce.Models.Admin
{
    public class CategoryModel
    {
        // VIEW Models - For JavaScript/Frontend Input
        public class CategoryListInputVIEW
        {
            [Display(Name = "Search Text")]
            public string SearchText { get; set; } = string.Empty;

            [Display(Name = "Filter Category IDs")]
            public string FilterCategoryIDs { get; set; } = string.Empty;

            [Display(Name = "Page Index")]
            [GreaterThanZero]
            public int PageIndex { get; set; } = 1;

            [Display(Name = "Page Size")]
            [GreaterThanZero]
            [Range(1, 100, ErrorMessage = "{0} must be between 1 and 100.")]
            public int PageSize { get; set; } = 10;

            [Display(Name = "Sort Column")]
            public string SortColumn { get; set; } = string.Empty;

            [Display(Name = "Sort Mode")]
            public string SortMode { get; set; } = string.Empty; // ASC / DESC
        }

        public class CategoryUpdateInputVIEW
        {
            [Display(Name = "Category ID")]
            public int CategoryID { get; set; } = 0;

            [Display(Name = "Category Name")]
            [Required(ErrorMessage = "{0} is required.")]
            [RequiredNotEmpty]
            [System.ComponentModel.DataAnnotations.MaxLength(255, ErrorMessage = "{0} cannot exceed 255 characters.")]
            public string CategoryName { get; set; } = string.Empty;

            [Display(Name = "Description")]
            [System.ComponentModel.DataAnnotations.MaxLength(500, ErrorMessage = "{0} cannot exceed 500 characters.")]
            public string Description { get; set; } = string.Empty;
        }

        public class CategoryDeleteInputVIEW
        {
            [Display(Name = "Category ID")]
            [Required(ErrorMessage = "{0} is required.")]
            [GreaterThanZero]
            public int CategoryID { get; set; }

            [Display(Name = "Cancelled Reason")]
            [System.ComponentModel.DataAnnotations.MaxLength(500, ErrorMessage = "{0} cannot exceed 500 characters.")]
            public string CancelledReason { get; set; } = string.Empty;
        }

        // Procedure Input Models - For Stored Procedures
        public class CategoryListInput
        {
            [Display(Name = "Search Text")]
            public string SearchText { get; set; } = string.Empty;

            [Display(Name = "Filter Category IDs")]
            public string FilterCategoryIDs { get; set; } = string.Empty;

            [Display(Name = "Page Index")]
            public int PageIndex { get; set; } = 1;

            [Display(Name = "Page Size")]
            public int PageSize { get; set; } = 10;

            [Display(Name = "Sort Column")]
            public string SortColumn { get; set; } = string.Empty;

            [Display(Name = "Sort Mode")]
            public string SortMode { get; set; } = string.Empty; // ASC / DESC
        }
        public class CategoryUpdateInput
        {
            [Display(Name = "User Action")]
            public int UserAction { get; set; } // 1 = Add, 2 = Edit

            [Display(Name = "Category ID")]
            public int CategoryID { get; set; } = 0;

            [Display(Name = "Category Name")]
            public string CategoryName { get; set; } = string.Empty;

            [Display(Name = "Description")]
            public string Description { get; set; } = string.Empty;

            [Display(Name = "Enter By")]
            public int EnterBy { get; set; } = 1; // TODO: Get from session/auth
        }

        public class CategoryDeleteInput
        {
            [Display(Name = "Category ID")]
            public int CategoryID { get; set; }

            [Display(Name = "Cancelled Reason")]
            public string CancelledReason { get; set; } = string.Empty;

            [Display(Name = "Enter By")]
            public int EnterBy { get; set; } = 1; // TODO: Get from session/auth
        }

        // Output Models from Stored Procedures
        public class Category
        {
            [Display(Name = "Category ID")]
            public int CategoryID { get; set; }

            [Display(Name = "Category Name")]
            public string CategoryName { get; set; } = string.Empty;

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
