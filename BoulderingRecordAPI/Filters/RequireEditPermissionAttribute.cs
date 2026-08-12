using Microsoft.AspNetCore.Mvc;

namespace BoulderingRecordAPI.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireEditPermissionAttribute : TypeFilterAttribute
{
    public RequireEditPermissionAttribute() : base(typeof(EditPermissionAuthorizationFilter))
    {
        Order = 1;
    }
}
