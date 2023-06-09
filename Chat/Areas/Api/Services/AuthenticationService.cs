using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;
using Chat.Areas.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Chat.Areas.Api.Services;

public interface IAuthenticationService
{
    public Task<IdentityResult> Register(string username, string email, string password);

    public Task<string> Login(string username, string password);
    public Task<TokenValidationResult> ValidateToken(string token);
}

public class AuthenticationService : IAuthenticationService
{
    private const int ExpirationDay = 30;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly string? _secretKey;
    private readonly UserManager<User> _userManager;

    public AuthenticationService(ILogger<AuthenticationService> logger, UserManager<User> userManager)
    {
        var secretKey = Environment.GetEnvironmentVariable("DEV_SECRET_KEY");
        _logger = logger;
        _userManager = userManager;
        _secretKey = secretKey;
    }

    /// <summary>
    ///     Create the specified user with given username and password
    /// </summary>
    /// <param name="username">username</param>
    /// <param name="password">password</param>
    /// <returns>The <see cref="IdentityResult" /> that indicates success or failure.</returns>
    /// <exception cref="ArgumentException">If username already exists</exception>
    public async Task<IdentityResult> Register(string username, string email, string password)
    {
        // Check if username already exists
        var user = await _userManager.FindByNameAsync(username);

        if (user != null)
        {
            throw new ArgumentException("User " + username + " already exists");
        }
        var result = await _userManager.CreateAsync(new User { UserName = username, Email = email }, password);
        return result;
    }

    /// <summary>
    ///     Tries to login with the given username and password. Retrieves the token if the authentication succeeds.
    /// </summary>
    /// <param name="username">username</param>
    /// <param name="password">password</param>
    /// <returns>A JSON Web Token that authenticates the user</returns>
    /// <exception cref="AuthenticationException">If the authentication fails</exception>
    public async Task<string> Login(string username, string password)
    {
        var user = await _userManager.FindByNameAsync(username);
        if (user == null) throw new AuthenticationException("The username or password is incorrect.");

        var result = await _userManager.CheckPasswordAsync(user, password);
        if (!result) throw new AuthenticationException("The username or password is incorrect.");
        return GenerateToken(user);
    }

    /// <summary>
    /// Generate the JWT token associated with <see cref="IdentityUser"/>
    /// </summary>
    /// <param name="user">user</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public string GenerateToken(IdentityUser user)
    {
        var expiration = DateTime.UtcNow.AddDays(ExpirationDay);
        if (user.UserName == null)
            throw new ArgumentException("Username is null");

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Iss, "chat"),
            new(JwtRegisteredClaimNames.Aud, "chat"),
            new(JwtRegisteredClaimNames.Sub, user.UserName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString(CultureInfo.InvariantCulture))
        };

        if (_secretKey == null)
            throw new ArgumentException("DEV_SECRET_KEY is not set");

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey)),
            SecurityAlgorithms.HmacSha256Signature
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = new JwtSecurityToken("chat", "chat", claims, expires: expiration,
            signingCredentials: signingCredentials);

        var token = tokenHandler.WriteToken(jwtToken);
        return token;
    }

    /// <summary>
    /// Validate the given token
    /// </summary>
    /// <param name="token">JWT token</param>
    public async Task<TokenValidationResult> ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = GetValidationParameters();

        var result = await tokenHandler.ValidateTokenAsync(token, validationParameters);
        return result;
    }

    private TokenValidationParameters GetValidationParameters()
    {
        if (_secretKey == null)
            throw new ArgumentException("DEV_SECRET_KEY is not set");
        return new TokenValidationParameters()
        {
            ValidateLifetime = false,
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidIssuer = "chat",
            ValidAudience = "chat",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey!)) // The same key as the one that generate the token
        };
    }
}