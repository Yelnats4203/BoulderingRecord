using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BoulderingRecordAPI.Entities;
using BoulderingRecordAPI.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BoulderingRecordAPI.Services;

public class TokenService(IOptions<JwtSettings> jwtOptions) : ITokenService
{
    private readonly JwtSettings _settings = jwtOptions.Value;

    public (string Token, DateTimeOffset ExpiresAt) GenerateToken(User user)
    {
        DateTimeOffset expiresAt = DateTimeOffset.UtcNow.AddMinutes(_settings.AccessTokenExpiresMinutes);

        Claim[] claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(TokenClaimTypes.Acc, user.Acc),
            new Claim(ClaimTypes.Name, user.Username),
        };

        SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        string tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return (tokenString, expiresAt);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

        TokenValidationParameters parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _settings.Issuer,
            ValidateAudience = true,
            ValidAudience = _settings.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ClockSkew = TimeSpan.Zero,
        };

        try
        {
            return handler.ValidateToken(token, parameters, out _);
        }
        catch (SecurityTokenException)
        {
            return null;
        }
        catch (ArgumentException)
        {
            return null;
        }
    }
}
