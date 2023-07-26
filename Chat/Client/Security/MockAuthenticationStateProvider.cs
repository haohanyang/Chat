using System.Net.Http.Json;
using System.Security.Claims;
using Chat.Shared.Dto;
using Microsoft.AspNetCore.Components.Authorization;

namespace Chat.Client.Security;

public class MockAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly HttpClient _httpClient;
    private readonly AuthenticationState _anonymousState;

    public MockAuthenticationStateProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _anonymousState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // API provided by mock service worker only
        var response = await _httpClient.GetAsync("/api/v1/auth");
        if (!response.IsSuccessStatusCode)
        {
            return _anonymousState;
        }

        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        if (user == null)
        {
            return _anonymousState;
        }
        var identity = new ClaimsIdentity(new[]
        {
            new Claim("sub", user.Id),
            new Claim("extension_Username", user.UserName),
            new Claim(ClaimTypes.Surname, user.LastName),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim("avatar", user.Avatar)
        }, "auth");

        var principal = new ClaimsPrincipal(identity);
        return new AuthenticationState(principal);
    }
}