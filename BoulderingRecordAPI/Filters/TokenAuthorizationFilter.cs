using System.Security.Claims;
using BoulderingRecordAPI.Models.Auth;
using BoulderingRecordAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BoulderingRecordAPI.Filters;

public class TokenAuthorizationFilter(ITokenService tokenService, IActiveTokenStore tokenStore)
    : IAsyncAuthorizationFilter
{
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        string header = context.HttpContext.Request.Headers.Authorization.ToString();

        if (string.IsNullOrEmpty(header) || !header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            context.Result = new UnauthorizedObjectResult(new UnauthorizedErrorResponse(UnauthorizedReason.SessionExpired));
            return Task.CompletedTask;
        }

        string token = header["Bearer ".Length..].Trim();

        ClaimsPrincipal? principal = tokenService.ValidateToken(token);
        if (principal is null)
        {
            context.Result = new UnauthorizedObjectResult(new UnauthorizedErrorResponse(UnauthorizedReason.SessionExpired));
            return Task.CompletedTask;
        }

        string? acc = principal.FindFirst(TokenClaimTypes.Acc)?.Value;
        if (string.IsNullOrEmpty(acc))
        {
            context.Result = new UnauthorizedObjectResult(new UnauthorizedErrorResponse(UnauthorizedReason.SessionExpired));
            return Task.CompletedTask;
        }

        if (!tokenStore.TryGetActiveToken(acc, out string? activeToken) ||
            !string.Equals(activeToken, token, StringComparison.Ordinal))
        {
            context.Result = new UnauthorizedObjectResult(new UnauthorizedErrorResponse(UnauthorizedReason.DuplicateLogin));
            return Task.CompletedTask;
        }

        context.HttpContext.User = principal;

        return Task.CompletedTask;
    }
}
