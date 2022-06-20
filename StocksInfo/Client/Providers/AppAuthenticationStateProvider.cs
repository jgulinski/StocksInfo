using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace Client.Providers;

public class AppAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorageService;
    private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler = new();
    
    public AppAuthenticationStateProvider(ILocalStorageService localStorageService)
    {
        _localStorageService = localStorageService;
    }
    
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var savedToken = await _localStorageService.GetItemAsync<string>("bearerToken");

            if (string.IsNullOrWhiteSpace(savedToken))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var jwtSecurityToken = _jwtSecurityTokenHandler.ReadJwtToken(savedToken);
            var expiry = jwtSecurityToken.ValidTo;

            if (expiry < DateTime.UtcNow)
            {
                await _localStorageService.RemoveItemAsync("bearerToken");
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
            
            var claims = ParseClaims(jwtSecurityToken);
            var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
            return new AuthenticationState(user);
        }
        catch (Exception)
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    private IEnumerable<Claim> ParseClaims(JwtSecurityToken jwtSecurityToken)
    {
        IList<Claim> claims = jwtSecurityToken.Claims.ToList();
        claims.Add(new Claim(ClaimTypes.Name, jwtSecurityToken.Subject));
        return claims;
    }

    internal async Task SignIn()
    {
        var savedToken = await _localStorageService.GetItemAsync<string>("bearerToken");
        var jwtSecurityToken = _jwtSecurityTokenHandler.ReadJwtToken(savedToken);
        var claims = ParseClaims(jwtSecurityToken);
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));

        Task<AuthenticationState> authenticationState = Task.FromResult(new AuthenticationState(user));
        NotifyAuthenticationStateChanged(authenticationState);
    }

    internal void SignOut()
    {
        var nobody = new ClaimsPrincipal(new ClaimsIdentity());
        
        Task<AuthenticationState> authenticationState = Task.FromResult(new AuthenticationState(nobody));
        NotifyAuthenticationStateChanged(authenticationState);
    }
    
}