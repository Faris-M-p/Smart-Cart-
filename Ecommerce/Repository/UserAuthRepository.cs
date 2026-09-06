using System.Data;
using Dapper;
using Ecommerce.Helpers.Common;
using Ecommerce.Helpers.UserAuth;
using Ecommerce.Interface;
using Microsoft.AspNetCore.Identity;
using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.UserAuthModel;

namespace Ecommerce.Repository
{
    public class UserAuthRepository : UserAuthInterface
    {
        private readonly IDataAccessDapper _dapper;
        private readonly IPasswordHasher<StorefrontUser> _passwordHasher;
        private readonly UserJwtTokenService _jwtTokenService;

        public UserAuthRepository(
            IDataAccessDapper dapper,
            IPasswordHasher<StorefrontUser> passwordHasher,
            UserJwtTokenService jwtTokenService)
        {
            _dapper = dapper;
            _passwordHasher = passwordHasher;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<CommonResponse> RegisterAsync(UserRegisterInput input)
        {
            var name = UserAuthHelper.NormalizeName(input.Name);
            var email = UserAuthHelper.NormalizeEmail(input.Email);
            var password = input.Password ?? string.Empty;

            if (name.Length < 2)
            {
                return Fail("Please enter your name.");
            }

            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            {
                return Fail("Please enter a valid email.");
            }

            if (password.Length < 6)
            {
                return Fail("Password must be at least 6 characters.");
            }

            var hash = _passwordHasher.HashPassword(new StorefrontUser { Email = email }, password);
            return await _dapper.ExecuteStoredProcedure("RegisterUser", new
            {
                FullName = name,
                Email = email,
                PasswordHash = hash
            });
        }

        public async Task<(CommonResponse Response, UserLoginData? Login)> LoginAsync(UserLoginInput input)
        {
            var email = UserAuthHelper.NormalizeEmail(input.Email);
            var password = input.Password ?? string.Empty;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password))
            {
                return (Fail(UserAuthHelper.InvalidCredentialsMessage), null);
            }

            using var connection = _dapper.CreateConnection();
            var user = await connection.QueryFirstOrDefaultAsync<UserAuthRow>(
                "GetUserByEmail",
                new { Email = email },
                commandType: CommandType.StoredProcedure);

            if (user == null || user.Cancelled || string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                return (Fail(UserAuthHelper.InvalidCredentialsMessage), null);
            }

            var verification = _passwordHasher.VerifyHashedPassword(
                new StorefrontUser { UserId = user.UserId, Email = user.Email },
                user.PasswordHash,
                password);

            if (verification == PasswordVerificationResult.Failed)
            {
                return (Fail(UserAuthHelper.InvalidCredentialsMessage), null);
            }

            var (token, expiresAt) = _jwtTokenService.CreateToken(user.UserId, user.FullName, user.Email);
            return (new CommonResponse
            {
                ResponseCode = user.UserId,
                StatusCode = true,
                ResponseMsg = UserAuthHelper.LoginSuccessMessage
            }, new UserLoginData
            {
                AccessToken = token,
                ExpiresAt = expiresAt,
                User = new UserSessionData
                {
                    UserId = user.UserId,
                    Name = user.FullName,
                    Email = user.Email
                }
            });
        }

        public async Task<UserSessionData?> GetCurrentAsync(int userId)
        {
            if (userId < 1)
            {
                return null;
            }

            using var connection = _dapper.CreateConnection();
            var user = await connection.QueryFirstOrDefaultAsync<UserAuthRow>(
                "GetUserById",
                new { UserId = userId },
                commandType: CommandType.StoredProcedure);

            if (user == null || user.Cancelled)
            {
                return null;
            }

            return new UserSessionData
            {
                UserId = user.UserId,
                Name = user.FullName,
                Email = user.Email
            };
        }

        private static CommonResponse Fail(string message) =>
            new()
            {
                ResponseCode = -1,
                StatusCode = false,
                ResponseMsg = message
            };
    }
}
