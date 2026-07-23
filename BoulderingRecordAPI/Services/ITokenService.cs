using System.Security.Claims;
using BoulderingRecordAPI.Entities;

namespace BoulderingRecordAPI.Services;

public interface ITokenService
{
    (string Token, DateTimeOffset ExpiresAt) GenerateToken(User user);

    ClaimsPrincipal? ValidateToken(string token);
}
