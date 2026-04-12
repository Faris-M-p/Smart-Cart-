using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.Admin.ProductModel;

namespace Ecommerce.Interface.Admin
{
    public interface IProductInterface
    {
        Task<TableOutput<Product>> GetProductListAsync(ProductListInput input);

        Task<Product?> GetProductByIdAsync(int id);

        Task<CommonResponse> CreateProductAsync(ProductUpdateInput input);

        Task<CommonResponse> UpdateProductAsync(ProductUpdateInput input);

        Task<CommonResponse> DeleteProductAsync(ProductDeleteInput input);
    }
}
