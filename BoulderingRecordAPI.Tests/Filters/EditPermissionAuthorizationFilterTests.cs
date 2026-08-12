using System.Security.Claims;
using BoulderingRecordAPI.Filters;
using BoulderingRecordAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BoulderingRecordAPI.Tests.Filters;

public class EditPermissionAuthorizationFilterTests
{
    private static AuthorizationFilterContext CreateContext(bool? hasEditPermission)
    {
        DefaultHttpContext httpContext = new DefaultHttpContext();
        if (hasEditPermission is not null)
        {
            ClaimsIdentity identity = new ClaimsIdentity(
                [new Claim(TokenClaimTypes.HasEditPermission, hasEditPermission.Value ? "true" : "false")], "Test");
            httpContext.User = new ClaimsPrincipal(identity);
        }

        ActionContext actionContext = new ActionContext(
            httpContext,
            new Microsoft.AspNetCore.Routing.RouteData(),
            new ActionDescriptor());

        return new AuthorizationFilterContext(actionContext, []);
    }

    [Fact]
    public async Task OnAuthorizationAsync_HasEditPermissionTrue_AllowsRequest()
    {
        EditPermissionAuthorizationFilter filter = new EditPermissionAuthorizationFilter();
        AuthorizationFilterContext context = CreateContext(hasEditPermission: true);

        await filter.OnAuthorizationAsync(context);

        Assert.Null(context.Result);
    }

    [Fact]
    public async Task OnAuthorizationAsync_HasEditPermissionFalse_ReturnsForbidden()
    {
        EditPermissionAuthorizationFilter filter = new EditPermissionAuthorizationFilter();
        AuthorizationFilterContext context = CreateContext(hasEditPermission: false);

        await filter.OnAuthorizationAsync(context);

        StatusCodeResult result = Assert.IsType<StatusCodeResult>(context.Result);
        Assert.Equal(StatusCodes.Status403Forbidden, result.StatusCode);
    }

    [Fact]
    public async Task OnAuthorizationAsync_NoClaim_ReturnsForbidden()
    {
        EditPermissionAuthorizationFilter filter = new EditPermissionAuthorizationFilter();
        AuthorizationFilterContext context = CreateContext(hasEditPermission: null);

        await filter.OnAuthorizationAsync(context);

        StatusCodeResult result = Assert.IsType<StatusCodeResult>(context.Result);
        Assert.Equal(StatusCodes.Status403Forbidden, result.StatusCode);
    }
}
