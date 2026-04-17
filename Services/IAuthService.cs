using CSMTutorial.Models;
 
namespace CSMTutorial.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(LoginModel model);
        Task<AuthResponse> RegisterAsync(RegisterModel model);
        Task<AuthResponse> RefreshTokenAsync(string refreshToken);
        Task LogoutAsync();

        Task<AuthResponse> ForgotPasswordAsync(string email);
        Task<AuthResponse> ResetPasswordAsync(ResetPasswordModel model);
        Task<AuthResponse> ChangePasswordAsync(int userId, ChangePasswordModel model);

        Task<IEnumerable<LoginHistory>> GetLoginHistoryAsync(int userId);

        // Current user state
        bool IsAuthenticated { get; }
        bool IsAdmin { get; }
        UserDto? CurrentUser { get; }
        Task<UserDto?> GetCurrentUserAsync();
        Task SetAuthStateAsync(TokenInfo tokenInfo);
        Task ClearAuthStateAsync();
    }
}
