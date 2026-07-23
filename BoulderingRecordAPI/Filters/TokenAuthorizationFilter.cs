using BoulderingRecordAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BoulderingRecordAPI.Filters;

public class TokenAuthorizationFilter(ITokenService tokenService, IActiveTokenStore tokenStore)
    : IAsyncAuthorizationFilter
{
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var header = context.HttpContext.Request.Headers.Authorization.ToString();

        if (string.IsNullOrEmpty(header) || !header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            context.Result = new UnauthorizedResult();
            return Task.CompletedTask;
        }

        var token = header["Bearer ".Length..].Trim();

        var principal = tokenService.ValidateToken(token);
        if (principal is null)
        {
            context.Result = new UnauthorizedResult();
            return Task.CompletedTask;
        }

        var acc = principal.FindFirst(TokenClaimTypes.Acc)?.Value;
        if (string.IsNullOrEmpty(acc))
        {
            context.Result = new UnauthorizedResult();
            return Task.CompletedTask;
        }

        if (!tokenStore.TryGetActiveToken(acc, out var activeToken) ||
            !string.Equals(activeToken, token, StringComparison.Ordinal))
        {
            context.Result = new UnauthorizedResult();
            return Task.CompletedTask;
        }

        context.HttpContext.User = principal;

        return Task.CompletedTask;
    }
}
