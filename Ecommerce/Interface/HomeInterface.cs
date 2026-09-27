using Ecommerce.Models;

namespace Ecommerce.Interface
{
    public interface HomeInterface
    {
        Task<HomeViewModel> GetHomePageDataAsync();
    }
}
