 
using CSMTutorial.Models;


namespace CSMTutorial.Data.Repositories;

public interface IUserRepository
{
    Task<User?> GetByLoginNameAsync(string loginName);
    Task<User?> GetByIdAsync(int userId);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByRefreshTokenAsync(string refreshToken);
    Task<IEnumerable<User>> GetAllAsync();

    Task<bool> UsernameExistsAsync(string username);
    Task<bool> EmailExistsAsync(string email);

    Task<int> CreateUserAsync(User user);
    Task<bool> UpdateUserAsync(User user);
    Task<bool> UpdatePasswordAsync(int userId, string hashedPassword);
    Task<bool> UpdateRefreshTokenAsync(int userId, string refreshToken, DateTime expiry);
    Task<bool> ClearRefreshTokenAsync(int userId);
    Task<bool> UpdateLastLoginAsync(int userId);
    Task<bool> IncrementFailedLoginAttemptsAsync(int userId);
    Task<bool> ResetFailedLoginAttemptsAsync(int userId);
    Task<bool> LockUserAsync(int userId, DateTime lockoutEnd);
    Task<bool> UnlockUserAsync(int userId);

    // Password reset
    Task<bool> UpdatePasswordResetTokenAsync(int userId, string token, DateTime expiry);
    Task<User?> GetByPasswordResetTokenAsync(string token);
    Task<bool> ClearPasswordResetTokenAsync(int userId);

    // Login history
    Task<int> LogLoginAttemptAsync(LoginHistory history);
    Task<bool> UpdateLogoutTimeAsync(int loginHistoryId);
    Task<IEnumerable<LoginHistory>> GetLoginHistoryAsync(int userId, int count = 10);
}