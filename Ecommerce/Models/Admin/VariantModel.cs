using System.ComponentModel.DataAnnotations;
using Ecommerce.CustomModelValidation;

namespace Ecommerce.Models.Admin
{
    public class VariantModel
    {
        // VIEW Models - For JavaScript/Frontend Input
        public class VariantListInputVIEW
        {
            [Display(Name = "Search Text")]
            public string SearchText { get; set; } = string.Empty;

            [Display(Name = "Filter Variant IDs")]
            public string FilterVariantIDs { get; set; } = string.Empty;

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

        public class VariantUpdateInputVIEW
        {
            [Display(Name = "Variant ID")]
            public int ID_Variant { get; set; } = 0;

            [Display(Name = "Variant Name")]
            [Required(ErrorMessage = "{0} is required.")]
            [RequiredNotEmpty]
            [System.ComponentModel.DataAnnotations.MaxLength(100, ErrorMessage = "{0} cannot exceed 100 characters.")]
            public string VariantName { get; set; } = string.Empty;

            [Display(Name = "Description")]
            [System.ComponentModel.DataAnnotations.MaxLength(500, ErrorMessage = "{0} cannot exceed 500 characters.")]
            public string Description { get; set; } = string.Empty;

            [Display(Name = "Display Order")]
            [GreaterThanZero]
            public int DisplayOrder { get; set; } = 1;
        }

        public class VariantDeleteInputVIEW
        {
            [Display(Name = "Variant ID")]
            [GreaterThanZero]
            public int ID_Variant { get; set; }

            [Display(Name = "Cancelled Reason")]
            [Required(ErrorMessage = "{0} is required.")]
            [RequiredNotEmpty]
            [System.ComponentModel.DataAnnotations.MaxLength(500, ErrorMessage = "{0} cannot exceed 500 characters.")]
            public string CancelledReason { get; set; } = string.Empty;
        }

        // Procedure Input Models - For Database Interaction
        public class VariantListInput
        {
            public string SearchText { get; set; } = string.Empty;
            public string FilterVariantIDs { get; set; } = string.Empty;
            public int PageIndex { get; set; } = 1;
            public int PageSize { get; set; } = 10;
            public string SortColumn { get; set; } = string.Empty;
            public string SortMode { get; set; } = string.Empty;
        }

        public class VariantUpdateInput
        {
            public int UserAction { get; set; } // 1=Insert, 2=Update, 3=Delete
            public int ID_Variant { get; set; } = 0;
            public string VariantName { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public int DisplayOrder { get; set; } = 1;
            public int EnterBy { get; set; } = 1; // TODO: Get from session/auth
            public string CancelledReason { get; set; } = string.Empty;
        }

        // Output Models
        public class VariantListOutput
        {
            public int ID_Variant { get; set; }
            public string VariantName { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public int DisplayOrder { get; set; }
            public DateTime CreatedOn { get; set; }
            public bool Cancelled { get; set; }
            public DateTime? CancelledOn { get; set; }
            public string CancelledReason { get; set; } = string.Empty;
        }

        public class VariantSelectByIdOutput
        {
            public int ID_Variant { get; set; }
            public string VariantName { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public int DisplayOrder { get; set; }
            public DateTime CreatedOn { get; set; }
            public bool Cancelled { get; set; }
            public DateTime? CancelledOn { get; set; }
            public string CancelledReason { get; set; } = string.Empty;
        }
    }
}
