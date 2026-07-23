using BoulderingRecordAPI.Entities;
using BoulderingRecordAPI.Filters;
using BoulderingRecordAPI.Options;
using BoulderingRecordAPI.Services;
using BoulderingRecordAPI.Tests.Fakes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BoulderingRecordAPI.Tests.Filters;

public class TokenAuthorizationFilterTests
{
    private const string TestAcc = "climber01";

    private static ITokenService CreateTokenService(int accessTokenExpiresMinutes = 120) => new TokenService(Microsoft.Extensions.Options.Options.Create(new JwtSettings
    {
        Key = "test-secret-key-for-unit-tests-1234567890",
        Issuer = "TestIssuer",
        Audience = "TestAudience",
        AccessTokenExpiresMinutes = accessTokenExpiresMinutes,
    }));

    private static User CreateTestUser() => new() { Username = "測試攀岩者", Acc = TestAcc };

    private static AuthorizationFilterContext CreateContext(string? authorizationHeader)
    {
        var httpContext = new DefaultHttpContext();
        if (authorizationHeader is not null)
        {
            httpContext.Request.Headers.Authorization = authorizationHeader;
        }

        var actionContext = new Microsoft.AspNetCore.Mvc.ActionContext(
            httpContext,
            new Microsoft.AspNetCore.Routing.RouteData(),
            new ActionDescriptor());

        return new AuthorizationFilterContext(actionContext, []);
    }

    [Fact]
    public async Task OnAuthorizationAsync_NoAuthorizationHeader_ReturnsUnauthorized()
    {
        var filter = new TokenAuthorizationFilter(CreateTokenService(), new FakeActiveTokenStore());
        var context = CreateContext(authorizationHeader: null);

        await filter.OnAuthorizationAsync(context);

        Assert.IsType<Microsoft.AspNetCore.Mvc.UnauthorizedResult>(context.Result);
    }

    [Fact]
    public async Task OnAuthorizationAsync_ExpiredToken_ReturnsUnauthorized()
    {
        var tokenService = CreateTokenService(accessTokenExpiresMinutes: -10);
        var tokenStore = new FakeActiveTokenStore();
        var (token, expiresAt) = tokenService.GenerateToken(CreateTestUser());
        tokenStore.SetActiveToken(TestAcc, token, expiresAt);

        var filter = new TokenAuthorizationFilter(tokenService, tokenStore);
        var context = CreateContext($"Bearer {token}");

        await filter.OnAuthorizationAsync(context);

        Assert.IsType<Microsoft.AspNetCore.Mvc.UnauthorizedResult>(context.Result);
    }

    [Fact]
    public async Task OnAuthorizationAsync_TamperedToken_ReturnsUnauthorized()
    {
        var tokenService = CreateTokenService();
        var tokenStore = new FakeActiveTokenStore();
        var (token, expiresAt) = tokenService.GenerateToken(CreateTestUser());
        tokenStore.SetActiveToken(TestAcc, token, expiresAt);
        var tamperedToken = token[..^1] + (token[^1] == 'A' ? 'B' : 'A');

        var filter = new TokenAuthorizationFilter(tokenService, tokenStore);
        var context = CreateContext($"Bearer {tamperedToken}");

        await filter.OnAuthorizationAsync(context);

        Assert.IsType<Microsoft.AspNetCore.Mvc.UnauthorizedResult>(context.Result);
    }

    [Fact]
    public async Task OnAuthorizationAsync_ValidTokenNotMatchingCache_ReturnsUnauthorized()
    {
        var tokenService = CreateTokenService();
        var tokenStore = new FakeActiveTokenStore();
        var (oldToken, _) = tokenService.GenerateToken(CreateTestUser());
        // 模擬已被新登入取代：快取內是另一組 token
        var (newToken, expiresAt) = tokenService.GenerateToken(CreateTestUser());
        tokenStore.SetActiveToken(TestAcc, newToken, expiresAt);

        var filter = new TokenAuthorizationFilter(tokenService, tokenStore);
        var context = CreateContext($"Bearer {oldToken}");

        await filter.OnAuthorizationAsync(context);

        Assert.IsType<Microsoft.AspNetCore.Mvc.UnauthorizedResult>(context.Result);
        Assert.Null(context.HttpContext.User.Identity?.Name);
    }

    [Fact]
    public async Task OnAuthorizationAsync_ValidTokenMatchingCache_SetsHttpContextUser()
    {
        var tokenService = CreateTokenService();
        var tokenStore = new FakeActiveTokenStore();
        var (token, expiresAt) = tokenService.GenerateToken(CreateTestUser());
        tokenStore.SetActiveToken(TestAcc, token, expiresAt);

        var filter = new TokenAuthorizationFilter(tokenService, tokenStore);
        var context = CreateContext($"Bearer {token}");

        await filter.OnAuthorizationAsync(context);

        Assert.Null(context.Result);
        Assert.Equal(TestAcc, context.HttpContext.User.FindFirst(TokenClaimTypes.Acc)?.Value);
    }
}
