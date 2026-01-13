using Ecommerce.Models;
using static Ecommerce.Models.ProductModel;

namespace Ecommerce.Interface
{
    public interface ShopInterface
    {

        Task<CommonModel.TableOutput<Product>> GetProductListAsync( InputProduct input);
    }
}
