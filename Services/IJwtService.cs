using CSMTutorial.Models;
using System.Security.Claims;

namespace CSMTutorial.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        string GenerateRefreshToken();
        ClaimsPrincipal? ValidateToken(string token);
        bool IsTokenExpired(string token);
        UserDto? GetUserFromToken(string token);
    }
}
