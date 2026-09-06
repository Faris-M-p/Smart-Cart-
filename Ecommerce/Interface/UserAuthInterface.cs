using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.UserAuthModel;

namespace Ecommerce.Interface
{
    public interface UserAuthInterface
    {
        Task<CommonResponse> RegisterAsync(UserRegisterInput input);

        Task<(CommonResponse Response, UserLoginData? Login)> LoginAsync(UserLoginInput input);

        Task<UserSessionData?> GetCurrentAsync(int userId);
    }
}
