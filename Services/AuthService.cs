using Blazored.LocalStorage;
 
using CSMTutorial.Data.Repositories;
using CSMTutorial.Models;
 using Microsoft.Extensions.Options;

namespace CSMTutorial.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly ILocalStorageService _localStorage;
    private readonly JwtSettings _jwtSettings;
    private readonly AppSettings _appSettings;
    private readonly ILogger<AuthService> _logger;

    private UserDto? _currentUser;
    private string? _currentToken;

    private const string TOKEN_KEY = "authToken";
    private const string REFRESH_TOKEN_KEY = "refreshToken";
    private const string USER_KEY = "currentUser";

    public AuthService( IUserRepository userRepository, IJwtService jwtService, IEmailService emailService, ILocalStorageService localStorage, IOptions<JwtSettings> jwtSettings, IOptions<AppSettings> appSettings, ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _emailService = emailService;
        _localStorage = localStorage;
        _jwtSettings = jwtSettings.Value;
        _appSettings = appSettings.Value;
        _logger = logger;
    }

    public bool IsAuthenticated => _currentUser != null;
    public bool IsAdmin => _currentUser?.IsAdmin == true;
    public UserDto? CurrentUser => _currentUser;

    public async Task<AuthResponse> RegisterAsync(RegisterModel model)
    {
        try
        {
            _logger.LogInformation("Registration attempt for username: {Username}", model.Username);

            // Check if username exists
            if (await _userRepository.UsernameExistsAsync(model.Username))
            {
                _logger.LogWarning("Registration failed - Username already exists: {Username}", model.Username);
                return new AuthResponse
                {
                    Success = false,
                    Message = "Username is already taken. Please choose a different username."
                };
            }

            // Check if email exists
            if (await _userRepository.EmailExistsAsync(model.Email))
            {
                _logger.LogWarning("Registration failed - Email already exists: {Email}", model.Email);
                return new AuthResponse
                {
                    Success = false,
                    Message = "Email is already registered. Please use a different email or try to login."
                };
            }

            // Hash password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

            // Create user
            var user = new User
            {
                LoginName = model.Username,
                LoginPassword = hashedPassword,
                Email = model.Email,
                Role = "User",
                IsAdmin = 0,
                AccessI = 1,
                RecordStatus = 1,
                EmailVerified = 1,
                C1 = model.FullName ?? model.Username
            };

            var userId = await _userRepository.CreateUserAsync(user);

            if (userId > 0)
            {
                _logger.LogInformation("User registered successfully: {Username} (ID: {UserId})", model.Username, userId);

                // Send welcome email (async, don't wait)
                _ = _emailService.SendWelcomeEmailAsync(model.Email, model.Username);

                return new AuthResponse
                {
                    Success = true,
                    Message = "Registration successful! You can now login with your credentials.",
                    User = new UserDto
                    {
                        UserId = userId,
                        LoginName = model.Username,
                        Email = model.Email,
                        IsAdmin = false,
                        FullName = model.FullName ?? model.Username,
                        Role = "User"
                    }
                };
            }

            _logger.LogError("Registration failed - Database insert returned 0 for user: {Username}", model.Username);
            return new AuthResponse
            {
                Success = false,
                Message = "Failed to create account. Please try again."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for user: {Username}", model.Username);
            return new AuthResponse
            {
                Success = false,
                Message = "An error occurred during registration. Please try again."
            };
        }
    }

    public async Task<AuthResponse> LoginAsync(LoginModel model)
    {
        try
        {
            _logger.LogInformation("Login attempt for user: {Username}", model.Username);

            var user = await _userRepository.GetByLoginNameAsync(model.Username);

            // User not found
            if (user == null)
            {
                _logger.LogWarning("Login failed - User not found: {Username}", model.Username);
                await LogLoginAttemptAsync(0, model.Username, "Failed", "User not found");
                return new AuthResponse
                {
                    Success = false,
                    Message = "Invalid username or password."
                };
            }

            // Check if account is locked
            if (user.IsLocked == 1)
            {
                if (user.LockoutEndDate.HasValue && user.LockoutEndDate.Value > DateTime.Now)
                {
                    var remainingTime = user.LockoutEndDate.Value - DateTime.Now;
                    await LogLoginAttemptAsync(user.UserId, model.Username, "Locked", "Account is locked");
                    return new AuthResponse
                    {
                        Success = false,
                        Message = $"Account is locked. Please try again in {Math.Ceiling(remainingTime.TotalMinutes)} minutes."
                    };
                }
                else
                {
                    // Lockout period has expired, unlock the account
                    await _userRepository.UnlockUserAsync(user.UserId);
                }
            }

            // Validate password
            bool isValidPassword;
            if (user.LoginPassword?.StartsWith("$2") == true) // BCrypt hash
            {
                isValidPassword = BCrypt.Net.BCrypt.Verify(model.Password, user.LoginPassword);
            }
            else
            {
                // Legacy plain text comparison
                isValidPassword = user.LoginPassword == model.Password;
            }

            if (!isValidPassword)
            {
                await _userRepository.IncrementFailedLoginAttemptsAsync(user.UserId);

                var updatedUser = await _userRepository.GetByIdAsync(user.UserId);
                var attempts = updatedUser?.FailedLoginAttempts ?? 0;

                if (attempts >= _appSettings.MaxFailedLoginAttempts)
                {
                    var lockoutEnd = DateTime.Now.AddMinutes(_appSettings.LockoutDurationMinutes);
                    await _userRepository.LockUserAsync(user.UserId, lockoutEnd);
                    await LogLoginAttemptAsync(user.UserId, model.Username, "Locked", "Max failed attempts exceeded");

                    return new AuthResponse
                    {
                        Success = false,
                        Message = $"Account has been locked due to multiple failed login attempts. Please try again in {_appSettings.LockoutDurationMinutes} minutes."
                    };
                }

                await LogLoginAttemptAsync(user.UserId, model.Username, "Failed", "Invalid password");

                var attemptsRemaining = _appSettings.MaxFailedLoginAttempts - attempts;
                return new AuthResponse
                {
                    Success = false,
                    Message = $"Invalid username or password. {attemptsRemaining} attempt(s) remaining."
                };
            }

            // Successful login - Generate tokens
            var token = _jwtService.GenerateToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryInDays);

            // Save refresh token to database
            await _userRepository.UpdateRefreshTokenAsync(user.UserId, refreshToken, refreshTokenExpiry);
            await _userRepository.ResetFailedLoginAttemptsAsync(user.UserId);
            await _userRepository.UpdateLastLoginAsync(user.UserId);
            await LogLoginAttemptAsync(user.UserId, model.Username, "Success", null);

            var userDto = new UserDto
            {
                UserId = user.UserId,
                LoginName = user.LoginName,
                Email = user.Email,
                IsAdmin = user.IsAdmin == 1,
                FullName = user.C1 ?? user.LoginName,
                Role = user.Role ?? "User"
            };

            // Store in local storage
            await _localStorage.SetItemAsStringAsync(TOKEN_KEY, token);
            await _localStorage.SetItemAsStringAsync(REFRESH_TOKEN_KEY, refreshToken);
            await _localStorage.SetItemAsync(USER_KEY, userDto);

            // Set current user
            _currentUser = userDto;
            _currentToken = token;

            _logger.LogInformation("Login successful for user: {Username}", model.Username);

            return new AuthResponse
            {
                Success = true,
                Message = "Login successful!",
                Token = token,
                RefreshToken = refreshToken,
                TokenExpiry = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes),
                User = userDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {Username}", model.Username);
            return new AuthResponse
            {
                Success = false,
                Message = "An error occurred during login. Please try again."
            };
        }
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var user = await _userRepository.GetByRefreshTokenAsync(refreshToken);

            if (user == null)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Invalid or expired refresh token."
                };
            }

            // Generate new tokens
            var newToken = _jwtService.GenerateToken(user);
            var newRefreshToken = _jwtService.GenerateRefreshToken();
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryInDays);

            // Update refresh token in database
            await _userRepository.UpdateRefreshTokenAsync(user.UserId, newRefreshToken, refreshTokenExpiry);

            var userDto = new UserDto
            {
                UserId = user.UserId,
                LoginName = user.LoginName,
                Email = user.Email,
                IsAdmin = user.IsAdmin == 1,
                FullName = user.C1 ?? user.LoginName,
                Role = user.Role ?? "User"
            };

            // Store in local storage
            await _localStorage.SetItemAsStringAsync(TOKEN_KEY, newToken);
            await _localStorage.SetItemAsStringAsync(REFRESH_TOKEN_KEY, newRefreshToken);
            await _localStorage.SetItemAsync(USER_KEY, userDto);

            _currentUser = userDto;
            _currentToken = newToken;

            return new AuthResponse
            {
                Success = true,
                Token = newToken,
                RefreshToken = newRefreshToken,
                TokenExpiry = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes),
                User = userDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return new AuthResponse
            {
                Success = false,
                Message = "Failed to refresh token."
            };
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            if (_currentUser != null)
            {
                await _userRepository.ClearRefreshTokenAsync(_currentUser.UserId);
                _logger.LogInformation("User logged out: {Username}", _currentUser.LoginName);
            }

            await ClearAuthStateAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
        }
    }

    public async Task<AuthResponse> ForgotPasswordAsync(string email)
    {
        try
        {
            _logger.LogInformation("Password reset requested for email: {Email}", email);

            var user = await _userRepository.GetByEmailAsync(email);

            // Always return success to prevent email enumeration
            if (user == null)
            {
                return new AuthResponse
                {
                    Success = true,
                    Message = "If an account with that email exists, you will receive a password reset link shortly."
                };
            }

            // Generate reset token
            var token = GenerateSecureToken();
            var expiry = DateTime.Now.AddHours(_appSettings.PasswordResetTokenExpiryHours);

            await _userRepository.UpdatePasswordResetTokenAsync(user.UserId, token, expiry);

            // Create reset link
            var resetLink = $"{_appSettings.BaseUrl}/reset-password?token={token}&email={Uri.EscapeDataString(email)}";

            // Send email
            await _emailService.SendPasswordResetEmailAsync(email, user.LoginName, resetLink);

            return new AuthResponse
            {
                Success = true,
                Message = "If an account with that email exists, you will receive a password reset link shortly."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during forgot password for email: {Email}", email);
            return new AuthResponse
            {
                Success = false,
                Message = "An error occurred. Please try again."
            };
        }
    }

    public async Task<AuthResponse> ResetPasswordAsync(ResetPasswordModel model)
    {
        try
        {
            var user = await _userRepository.GetByPasswordResetTokenAsync(model.Token);

            if (user == null)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Invalid or expired password reset link. Please request a new one."
                };
            }

            if (!string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase))
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Invalid password reset link."
                };
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            await _userRepository.UpdatePasswordAsync(user.UserId, hashedPassword);

            // Send notification email
            if (!string.IsNullOrEmpty(user.Email))
            {
                _ = _emailService.SendPasswordChangedNotificationAsync(user.Email, user.LoginName);
            }

            _logger.LogInformation("Password reset successful for user: {Username}", user.LoginName);

            return new AuthResponse
            {
                Success = true,
                Message = "Password has been reset successfully. You can now login with your new password."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset");
            return new AuthResponse
            {
                Success = false,
                Message = "An error occurred. Please try again."
            };
        }
    }

    public async Task<AuthResponse> ChangePasswordAsync(int userId, ChangePasswordModel model)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            // Verify current password
            bool isValidPassword;
            if (user.LoginPassword?.StartsWith("$2") == true)
            {
                isValidPassword = BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.LoginPassword);
            }
            else
            {
                isValidPassword = user.LoginPassword == model.CurrentPassword;
            }

            if (!isValidPassword)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Current password is incorrect."
                };
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            await _userRepository.UpdatePasswordAsync(userId, hashedPassword);

            if (!string.IsNullOrEmpty(user.Email))
            {
                _ = _emailService.SendPasswordChangedNotificationAsync(user.Email, user.LoginName);
            }

            return new AuthResponse
            {
                Success = true,
                Message = "Password changed successfully."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user: {UserId}", userId);
            return new AuthResponse
            {
                Success = false,
                Message = "An error occurred. Please try again."
            };
        }
    }

    public async Task<IEnumerable<LoginHistory>> GetLoginHistoryAsync(int userId)
    {
        return await _userRepository.GetLoginHistoryAsync(userId);
    }

    public async Task<UserDto?> GetCurrentUserAsync()
    {
        if (_currentUser != null)
            return _currentUser;

        try
        {
            var token = await _localStorage.GetItemAsStringAsync(TOKEN_KEY);
            if (string.IsNullOrEmpty(token))
                return null;

            if (_jwtService.IsTokenExpired(token))
            {
                // Try to refresh
                var refreshToken = await _localStorage.GetItemAsStringAsync(REFRESH_TOKEN_KEY);
                if (!string.IsNullOrEmpty(refreshToken))
                {
                    var result = await RefreshTokenAsync(refreshToken);
                    if (result.Success)
                        return _currentUser;
                }

                await ClearAuthStateAsync();
                return null;
            }

            _currentUser = await _localStorage.GetItemAsync<UserDto>(USER_KEY);
            _currentToken = token;
            return _currentUser;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            return null;
        }
    }

    public async Task SetAuthStateAsync(TokenInfo tokenInfo)
    {
        try
        {
            await _localStorage.SetItemAsStringAsync(TOKEN_KEY, tokenInfo.Token);
            await _localStorage.SetItemAsStringAsync(REFRESH_TOKEN_KEY, tokenInfo.RefreshToken);
            await _localStorage.SetItemAsync(USER_KEY, tokenInfo.User);

            _currentUser = tokenInfo.User;
            _currentToken = tokenInfo.Token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting auth state");
        }
    }

    public async Task ClearAuthStateAsync()
    {
        try
        {
            await _localStorage.RemoveItemAsync(TOKEN_KEY);
            await _localStorage.RemoveItemAsync(REFRESH_TOKEN_KEY);
            await _localStorage.RemoveItemAsync(USER_KEY);

            _currentUser = null;
            _currentToken = null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing auth state");
        }
    }

    private async Task LogLoginAttemptAsync(int userId, string loginName, string status, string? reason)
    {
        try
        {
            var history = new LoginHistory
            {
                UserId = userId,
                LoginName = loginName,
                LoginStatus = status,
                FailureReason = reason
            };
            await _userRepository.LogLoginAttemptAsync(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging login attempt");
        }
    }

    private static string GenerateSecureToken()
    {
        var tokenBytes = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(tokenBytes);
        return Convert.ToBase64String(tokenBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }
}