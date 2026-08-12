using BoulderingRecordAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BoulderingRecordAPI.Filters;

public class EditPermissionAuthorizationFilter : IAsyncAuthorizationFilter
{
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        string? hasEditPermission = context.HttpContext.User.FindFirst(TokenClaimTypes.HasEditPermission)?.Value;
        if (!string.Equals(hasEditPermission, "true", StringComparison.OrdinalIgnoreCase))
        {
            context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
        }

        return Task.CompletedTask;
    }
}
