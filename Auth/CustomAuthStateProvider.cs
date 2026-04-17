using Blazored.LocalStorage;
 
using CSMTutorial.Services;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace CSMTutorial.Auth;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly IJwtService _jwtService;
    private readonly ILogger<CustomAuthStateProvider> _logger;

    private const string TOKEN_KEY = "authToken";
    private const string USER_KEY = "currentUser";

    public CustomAuthStateProvider(
        ILocalStorageService localStorage,
        IJwtService jwtService,
        ILogger<CustomAuthStateProvider> logger)
    {
        _localStorage = localStorage;
        _jwtService = jwtService;
        _logger = logger;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await _localStorage.GetItemAsStringAsync(TOKEN_KEY);

            if (string.IsNullOrEmpty(token))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            // Validate token
            var principal = _jwtService.ValidateToken(token);
            if (principal == null)
            {
                await _localStorage.RemoveItemAsync(TOKEN_KEY);
                await _localStorage.RemoveItemAsync(USER_KEY);
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            return new AuthenticationState(principal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting authentication state");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    public void NotifyAuthenticationStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task MarkUserAsAuthenticated(string token)
    {
        var principal = _jwtService.ValidateToken(token);
        if (principal != null)
        {
            var authState = new AuthenticationState(principal);
            NotifyAuthenticationStateChanged(Task.FromResult(authState));
        }
    }

    public async Task MarkUserAsLoggedOut()
    {
        await _localStorage.RemoveItemAsync(TOKEN_KEY);
        await _localStorage.RemoveItemAsync(USER_KEY);

        var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymousUser)));
    }
}