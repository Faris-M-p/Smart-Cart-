using System.ComponentModel.DataAnnotations;
using Ecommerce.CustomModelValidation;

namespace Ecommerce.Models.Admin
{
    public class ProductModel
    {
        // VIEW Models - For JavaScript/Frontend Input
        public class ProductListInputVIEW
        {
            [Display(Name = "Search Text")]
            public string SearchText { get; set; } = string.Empty;

            [Display(Name = "Filter Category IDs")]
            public string FilterCategoryIDs { get; set; } = string.Empty;

            [Display(Name = "Filter SubCategory IDs")]
            public string FilterSubCategoryIDs { get; set; } = string.Empty;

            [Display(Name = "Filter Brand IDs")]
            public string FilterBrandIDs { get; set; } = string.Empty;

            [Display(Name = "Filter Status IDs")]
            public string FilterStatusIDs { get; set; } = string.Empty;

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

        public class ProductUpdateInputVIEW
        {
            [Display(Name = "Product ID")]
            public int ID_Product { get; set; } = 0;

            [Display(Name = "Product Name")]
            [Required(ErrorMessage = "{0} is required.")]
            [RequiredNotEmpty]
            [System.ComponentModel.DataAnnotations.MaxLength(255, ErrorMessage = "{0} cannot exceed 255 characters.")]
            public string Name { get; set; } = string.Empty;

            [Display(Name = "Description")]
            public string Description { get; set; } = string.Empty;

            [Display(Name = "Price")]
            [Required(ErrorMessage = "{0} is required.")]
            [GreaterThanZero(ErrorMessage = "{0} must be greater than zero.")]
            public decimal Price { get; set; }

            [Display(Name = "MRP")]
            [GreaterThanZero(ErrorMessage = "{0} must be greater than zero.")]
            public decimal? MRP { get; set; }

            [Display(Name = "Category")]
            [Required(ErrorMessage = "{0} is required.")]
            [GreaterThanZero(ErrorMessage = "{0} must be selected.")]
            public int FK_Category { get; set; }

            [Display(Name = "SubCategory")]
            [Required(ErrorMessage = "{0} is required.")]
            [GreaterThanZero(ErrorMessage = "{0} must be selected.")]
            public int FK_SubCategory { get; set; }

            [Display(Name = "Brand")]
            public int? FK_Brand { get; set; }

            [Display(Name = "Rating")]
            [Range(0, 5, ErrorMessage = "{0} must be between 0 and 5.")]
            public decimal? Rating { get; set; }

            [Display(Name = "Gender")]
            [System.ComponentModel.DataAnnotations.MaxLength(50, ErrorMessage = "{0} cannot exceed 50 characters.")]
            public string? Gender { get; set; }

            [Display(Name = "Status")]
            [GreaterThanZero(ErrorMessage = "{0} must be selected.")]
            public int FK_Status { get; set; } = 1;
        }

        public class ProductDeleteInputVIEW
        {
            [Display(Name = "Product ID")]
            [Required(ErrorMessage = "{0} is required.")]
            [GreaterThanZero]
            public int ID_Product { get; set; }

            [Display(Name = "Cancelled Reason")]
            [System.ComponentModel.DataAnnotations.MaxLength(500, ErrorMessage = "{0} cannot exceed 500 characters.")]
            public string CancelledReason { get; set; } = string.Empty;
        }

        // Procedure Input Models - For Stored Procedures
        public class ProductListInput
        {
            [Display(Name = "Search Text")]
            public string SearchText { get; set; } = string.Empty;

            [Display(Name = "Filter Category IDs")]
            public string FilterCategoryIDs { get; set; } = string.Empty;

            [Display(Name = "Filter SubCategory IDs")]
            public string FilterSubCategoryIDs { get; set; } = string.Empty;

            [Display(Name = "Filter Brand IDs")]
            public string FilterBrandIDs { get; set; } = string.Empty;

            [Display(Name = "Filter Status IDs")]
            public string FilterStatusIDs { get; set; } = string.Empty;

            [Display(Name = "Page Index")]
            public int PageIndex { get; set; } = 1;

            [Display(Name = "Page Size")]
            public int PageSize { get; set; } = 10;

            [Display(Name = "Sort Column")]
            public int SortColumn { get; set; }

            [Display(Name = "Sort Mode")]
            public string SortMode { get; set; } = string.Empty; // ASC / DESC
        }

        public class ProductUpdateInput
        {
            [Display(Name = "User Action")]
            public int UserAction { get; set; } // 1 = Add, 2 = Edit

            [Display(Name = "Product ID")]
            public int ID_Product { get; set; } = 0;

            [Display(Name = "Product Name")]
            public string Name { get; set; } = string.Empty;

            [Display(Name = "Description")]
            public string Description { get; set; } = string.Empty;

            [Display(Name = "Price")]
            public decimal Price { get; set; }

            [Display(Name = "MRP")]
            public decimal? MRP { get; set; }

            [Display(Name = "Category")]
            public int FK_Category { get; set; }

            [Display(Name = "SubCategory")]
            public int FK_SubCategory { get; set; }

            [Display(Name = "Brand")]
            public int? FK_Brand { get; set; }

            [Display(Name = "Rating")]
            public decimal? Rating { get; set; }

            [Display(Name = "Gender")]
            public string? Gender { get; set; }

            [Display(Name = "Status")]
            public int FK_Status { get; set; } = 1;

            [Display(Name = "Enter By")]
            public int EnterBy { get; set; } = 1; // TODO: Get from session/auth
        }

        public class ProductDeleteInput
        {
            [Display(Name = "Product ID")]
            public int ID_Product { get; set; }

            [Display(Name = "Cancelled Reason")]
            public string CancelledReason { get; set; } = string.Empty;

            [Display(Name = "Enter By")]
            public int EnterBy { get; set; } = 1; // TODO: Get from session/auth
        }

        // Output Models from Stored Procedures
        public class Product
        {
            [Display(Name = "Product ID")]
            public int ID_Product { get; set; }

            [Display(Name = "Product Name")]
            public string Name { get; set; } = string.Empty;

            [Display(Name = "Description")]
            public string? Description { get; set; }

            [Display(Name = "Price")]
            public decimal Price { get; set; }

            [Display(Name = "MRP")]
            public decimal? MRP { get; set; }

            [Display(Name = "Category ID")]
            public int FK_Category { get; set; }

            [Display(Name = "SubCategory ID")]
            public int FK_SubCategory { get; set; }

            [Display(Name = "Brand ID")]
            public int? FK_Brand { get; set; }

            [Display(Name = "Rating")]
            public decimal? Rating { get; set; }

            [Display(Name = "Gender")]
            public string? Gender { get; set; }

            [Display(Name = "Status ID")]
            public int FK_Status { get; set; }

            [Display(Name = "Created On")]
            public DateTime? CreatedOn { get; set; }

            [Display(Name = "Updated On")]
            public DateTime? UpdatedOn { get; set; }

            [Display(Name = "Image Data")]
            public string? ImageData { get; set; }

            [Display(Name = "Is Base64")]
            public bool? IsBase64 { get; set; }
            public bool Cancelled { get; internal set; }
        }
    }
}
