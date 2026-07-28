using BoulderingRecordAPI.Entities;
using BoulderingRecordAPI.Filters;
using BoulderingRecordAPI.Models.Auth;
using BoulderingRecordAPI.Repositories;
using BoulderingRecordAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BoulderingRecordAPI.Controllers;

/// <summary>
/// 處理登入、登出與 token 換發等身分驗證相關端點。
/// </summary>
[ApiController]
[Route("[controller]")]
public class AuthController(
    IUserRepository userRepository,
    ITokenService tokenService,
    IActiveTokenStore tokenStore) : ControllerBase
{
    private static readonly PasswordHasher<User> PasswordHasher = new();

    /// <summary>
    /// 以帳號密碼登入，成功後回傳 JWT token 與到期時間，並將其設為該帳號的 active token。
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

    /// <summary>
    /// 登出，將目前帳號的 active token 從快取中移除。
    /// </summary>
    [TokenAuthorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

    /// <summary>
    /// 換發新 token，並取代快取中原有的 active token。
    /// </summary>
    [TokenAuthorize]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(RefreshTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
