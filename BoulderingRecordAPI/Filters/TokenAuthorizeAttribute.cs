using Microsoft.AspNetCore.Mvc;

namespace BoulderingRecordAPI.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class TokenAuthorizeAttribute : TypeFilterAttribute
{
    public TokenAuthorizeAttribute() : base(typeof(TokenAuthorizationFilter))
    {
    }
}
