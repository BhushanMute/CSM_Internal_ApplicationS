 
using CSMTutorial.Data;
using CSMTutorial.Data.Repositories;
using CSMTutorial.Models;
using Dapper;
using System.Data;

namespace CSMTutorial.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DapperContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(DapperContext context, ILogger<UserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<User?> GetByLoginNameAsync(string loginName)
    {
        try
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<User>(
                "sp_User_GetByLoginName",
                new { LoginName = loginName },
                commandType: CommandType.StoredProcedure);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user by login name: {LoginName}", loginName);
            throw;
        }
    }

    public async Task<User?> GetByIdAsync(int userId)
    {
        try
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<User>(
                "sp_User_GetById",
                new { UserId = userId },
                commandType: CommandType.StoredProcedure);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user by ID: {UserId}", userId);
            throw;
        }
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        try
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<User>(
                "sp_User_GetByEmail",
                new { Email = email },
                commandType: CommandType.StoredProcedure);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user by email: {Email}", email);
            throw;
        }
    }

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
    {
        try
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<User>(
                "sp_User_GetByRefreshToken",
                new { RefreshToken = refreshToken },
                commandType: CommandType.StoredProcedure);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user by refresh token");
            throw;
        }
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        try
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<User>(
                "sp_User_GetAll",
                commandType: CommandType.StoredProcedure);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all users");
            throw;
        }
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        try
        {
            using var connection = _context.CreateConnection();
            var count = await connection.ExecuteScalarAsync<int>(
                "sp_User_UsernameExists",
                new { Username = username },
                commandType: CommandType.StoredProcedure);
            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking username existence: {Username}", username);
            throw;
        }
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        try
        {
            using var connection = _context.CreateConnection();
            var count = await connection.ExecuteScalarAsync<int>(
                "sp_User_EmailExists",
                new { Email = email },
                commandType: CommandType.StoredProcedure);
            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking email existence: {Email}", email);
            throw;
        }
    }

    public async Task<int> CreateUserAsync(User user)
    {
        try
        {
            using var connection = _context.CreateConnection();
            var parameters = new
            {
                user.LoginName,
                user.LoginPassword,
                Role = user.Role ?? "User",
                IsAdmin = user.IsAdmin,
                AccessI = user.AccessI,
                RecordStatus = user.RecordStatus ?? 1,
                user.Email,
                EmailVerified = user.EmailVerified,
                C1 = user.C1 // Full Name
            };

            var userId = await connection.ExecuteScalarAsync<int>(
                "sp_User_Create",
                parameters,
                commandType: CommandType.StoredProcedure);

            _logger.LogInformation("Created new user: {LoginName} with ID: {UserId}", user.LoginName, userId);
            return userId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user: {LoginName}", user.LoginName);
            throw;
        }
    }

    public async Task<bool> UpdateUserAsync(User user)
    {
        try
        {
            using var connection = _context.CreateConnection();
            var parameters = new
            {
                user.UserId,
                user.Email,
                user.C1,
                user.Role
            };

            var affected = await connection.ExecuteAsync(
                "sp_User_Update",
                parameters,
                commandType: CommandType.StoredProcedure);
            return affected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", user.UserId);
            throw;
        }
    }

    public async Task<bool> UpdatePasswordAsync(int userId, string hashedPassword)
    {
        try
        {
            using var connection = _context.CreateConnection();
            var affected = await connection.ExecuteAsync(
                "sp_User_UpdatePassword",
                new { UserId = userId, Password = hashedPassword },
                commandType: CommandType.StoredProcedure);
            return affected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating password for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> UpdateRefreshTokenAsync(int userId, string refreshToken, DateTime expiry)
    {
        try
        {
            using var connection = _context.CreateConnection();
            var affected = await connection.ExecuteAsync(
                "sp_User_UpdateRefreshToken",
                new { UserId = userId, RefreshToken = refreshToken, Expiry = expiry },
                commandType: CommandType.StoredProcedure);
            return affected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating refresh token for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> ClearRefreshTokenAsync(int userId)
    {
        try
        {
            using var connection = _context.CreateConnection();
            var affected = await connection.ExecuteAsync(
                "sp_User_ClearRefreshToken",
                new { UserId = userId },
                commandType: CommandType.StoredProcedure);
            return affected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing refresh token for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> UpdateLastLoginAsync(int userId)
    {
        try
        {
            using var connection = _context.CreateConnection();
            var affected = await connection.ExecuteAsync(
                "sp_User_UpdateLastLogin",
                new { UserId = userId },
                commandType: CommandType.StoredProcedure);
            return affected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating last login for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> IncrementFailedLoginAttemptsAsync(int userId)
    {
        try
        {
            using var connection = _context.CreateConnection();
            var affected = await connection.ExecuteAsync(
                "sp_User_IncrementFailedLoginAttempts",
                new { UserId = userId },
                commandType: CommandType.StoredProcedure);
            return affected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing failed login attempts for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> ResetFailedLoginAttemptsAsync(int userId)
    {
        try
        {
            using var connection = _context.CreateConnection();
            var affected = await connection.ExecuteAsync(
                "sp_User_ResetFailedLoginAttempts",
                new { UserId = userId },
                commandType: CommandType.StoredProcedure);
            return affected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting failed login attempts for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> LockUserAsync(int userId, DateTime lockoutEnd)
    {
        try
        {
            using var connection = _context.CreateConnection();
            var affected = await connection.ExecuteAsync(
                "sp_User_Lock",
                new { UserId = userId, LockoutEnd = lockoutEnd },
                commandType: CommandType.StoredProcedure);

            _logger.LogWarning("User {UserId} has been locked until {LockoutEnd}", userId, lockoutEnd);
            return affected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error locking user: {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> UnlockUserAsync(int userId)
    {
        try
        {
            using var connection = _context.CreateConnection();
            var affected = await connection.ExecuteAsync(
                "sp_User_Unlock",
                new { UserId = userId },
                commandType: CommandType.StoredProcedure);
            return affected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlocking user: {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> UpdatePasswordResetTokenAsync(int userId, string token, DateTime expiry)
    {
        try
        {
            using var connection = _context.CreateConnection();
            var affected = await connection.ExecuteAsync(
                "sp_User_UpdatePasswordResetToken",
                new { UserId = userId, Token = token, Expiry = expiry },
                commandType: CommandType.StoredProcedure);
            return affected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating password reset token for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<User?> GetByPasswordResetTokenAsync(string token)
    {
        try
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<User>(
                "sp_User_GetByPasswordResetToken",
                new { Token = token },
                commandType: CommandType.StoredProcedure);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user by password reset token");
            throw;
        }
    }

    public async Task<bool> ClearPasswordResetTokenAsync(int userId)
    {
        try
        {
            using var connection = _context.CreateConnection();
            var affected = await connection.ExecuteAsync(
                "sp_User_ClearPasswordResetToken",
                new { UserId = userId },
                commandType: CommandType.StoredProcedure);
            return affected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing password reset token for user: {UserId}", userId);
            throw;
        }
    }

    // Login History Methods
    public async Task<int> LogLoginAttemptAsync(LoginHistory history)
    {
        try
        {
            using var connection = _context.CreateConnection();
            var parameters = new
            {
                history.UserId,
                history.LoginName,
                history.IPAddress,
                history.UserAgent,
                history.LoginStatus,
                history.FailureReason
            };

            return await connection.ExecuteScalarAsync<int>(
                "sp_LoginHistory_LogAttempt",
                parameters,
                commandType: CommandType.StoredProcedure);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging login attempt");
            return 0; // Don't throw, just log
        }
    }

    public async Task<bool> UpdateLogoutTimeAsync(int loginHistoryId)
    {
        try
        {
            using var connection = _context.CreateConnection();
            var affected = await connection.ExecuteAsync(
                "sp_LoginHistory_UpdateLogout",
                new { LoginHistoryId = loginHistoryId },
                commandType: CommandType.StoredProcedure);
            return affected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating logout time");
            return false;
        }
    }

    public async Task<IEnumerable<LoginHistory>> GetLoginHistoryAsync(int userId, int count = 10)
    {
        try
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<LoginHistory>(
                "sp_LoginHistory_GetByUserId",
                new { UserId = userId, Count = count },
                commandType: CommandType.StoredProcedure);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching login history for user: {UserId}", userId);
            return Enumerable.Empty<LoginHistory>();
        }
    }
}