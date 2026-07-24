using BoulderingRecordAPI.Entities;
using BoulderingRecordAPI.Filters;
using BoulderingRecordAPI.Models.Auth;
using BoulderingRecordAPI.Repositories;
using BoulderingRecordAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BoulderingRecordAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    IUserRepository userRepository,
    ITokenService tokenService,
    IActiveTokenStore tokenStore) : ControllerBase
{
    private static readonly PasswordHasher<User> PasswordHasher = new();

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetByAccAsync(request.Acc, cancellationToken);
        if (user is null)
        {
            return Unauthorized();
        }

        PasswordVerificationResult verificationResult = PasswordHasher.VerifyHashedPassword(user, user.Psw, request.Psw);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return Unauthorized();
        }

        (string token, DateTimeOffset expiresAt) = tokenService.GenerateToken(user);
        tokenStore.SetActiveToken(user.Acc, token, expiresAt);

        return Ok(new LoginResponse(token, expiresAt));
    }

    [TokenAuthorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        string? acc = User.FindFirst(TokenClaimTypes.Acc)?.Value;
        if (string.IsNullOrEmpty(acc))
        {
            return Unauthorized();
        }

        tokenStore.RemoveActiveToken(acc);
        return NoContent();
    }

    [TokenAuthorize]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        string? acc = User.FindFirst(TokenClaimTypes.Acc)?.Value;
        if (string.IsNullOrEmpty(acc))
        {
            return Unauthorized();
        }

        User? user = await userRepository.GetByAccAsync(acc, cancellationToken);
        if (user is null)
        {
            return Unauthorized();
        }

        (string newToken, DateTimeOffset expiresAt) = tokenService.GenerateToken(user);
        tokenStore.SetActiveToken(acc, newToken, expiresAt);

        return Ok(new RefreshTokenResponse(newToken, expiresAt));
    }
}
