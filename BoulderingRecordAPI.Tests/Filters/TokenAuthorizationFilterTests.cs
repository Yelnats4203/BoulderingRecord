using BoulderingRecordAPI.Entities;
using BoulderingRecordAPI.Filters;
using BoulderingRecordAPI.Models.Auth;
using BoulderingRecordAPI.Options;
using BoulderingRecordAPI.Services;
using BoulderingRecordAPI.Tests.Fakes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BoulderingRecordAPI.Tests.Filters;

public class TokenAuthorizationFilterTests
{
    private const string TestAcc = "climber01";

    private static void AssertUnauthorizedWithReason(Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext context, UnauthorizedReason expectedReason)
    {
        UnauthorizedObjectResult result = Assert.IsType<UnauthorizedObjectResult>(context.Result);
        UnauthorizedErrorResponse errorResponse = Assert.IsType<UnauthorizedErrorResponse>(result.Value);
        Assert.Equal(expectedReason, errorResponse.Reason);
    }

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
        DefaultHttpContext httpContext = new DefaultHttpContext();
        if (authorizationHeader is not null)
        {
            httpContext.Request.Headers.Authorization = authorizationHeader;
        }

        Microsoft.AspNetCore.Mvc.ActionContext actionContext = new Microsoft.AspNetCore.Mvc.ActionContext(
            httpContext,
            new Microsoft.AspNetCore.Routing.RouteData(),
            new ActionDescriptor());

        return new AuthorizationFilterContext(actionContext, []);
    }

    [Fact]
    public async Task OnAuthorizationAsync_NoAuthorizationHeader_ReturnsUnauthorizedWithSessionExpiredReason()
    {
        TokenAuthorizationFilter filter = new TokenAuthorizationFilter(CreateTokenService(), new FakeActiveTokenStore());
        AuthorizationFilterContext context = CreateContext(authorizationHeader: null);

        await filter.OnAuthorizationAsync(context);

        AssertUnauthorizedWithReason(context, UnauthorizedReason.SessionExpired);
    }

    [Fact]
    public async Task OnAuthorizationAsync_ExpiredToken_ReturnsUnauthorizedWithSessionExpiredReason()
    {
        ITokenService tokenService = CreateTokenService(accessTokenExpiresMinutes: -10);
        FakeActiveTokenStore tokenStore = new FakeActiveTokenStore();
        (string token, DateTimeOffset expiresAt) = tokenService.GenerateToken(CreateTestUser());
        tokenStore.SetActiveToken(TestAcc, token, expiresAt);

        TokenAuthorizationFilter filter = new TokenAuthorizationFilter(tokenService, tokenStore);
        AuthorizationFilterContext context = CreateContext($"Bearer {token}");

        await filter.OnAuthorizationAsync(context);

        AssertUnauthorizedWithReason(context, UnauthorizedReason.SessionExpired);
    }

    [Fact]
    public async Task OnAuthorizationAsync_TamperedToken_ReturnsUnauthorizedWithSessionExpiredReason()
    {
        ITokenService tokenService = CreateTokenService();
        FakeActiveTokenStore tokenStore = new FakeActiveTokenStore();
        (string token, DateTimeOffset expiresAt) = tokenService.GenerateToken(CreateTestUser());
        tokenStore.SetActiveToken(TestAcc, token, expiresAt);
        string tamperedToken = token[..^1] + (token[^1] == 'A' ? 'B' : 'A');

        TokenAuthorizationFilter filter = new TokenAuthorizationFilter(tokenService, tokenStore);
        AuthorizationFilterContext context = CreateContext($"Bearer {tamperedToken}");

        await filter.OnAuthorizationAsync(context);

        AssertUnauthorizedWithReason(context, UnauthorizedReason.SessionExpired);
    }

    [Fact]
    public async Task OnAuthorizationAsync_ValidTokenNotMatchingCache_ReturnsUnauthorizedWithDuplicateLoginReason()
    {
        ITokenService tokenService = CreateTokenService();
        FakeActiveTokenStore tokenStore = new FakeActiveTokenStore();
        (string oldToken, DateTimeOffset _) = tokenService.GenerateToken(CreateTestUser());
        // 模擬已被新登入取代：快取內是另一組 token
        (string newToken, DateTimeOffset expiresAt) = tokenService.GenerateToken(CreateTestUser());
        tokenStore.SetActiveToken(TestAcc, newToken, expiresAt);

        TokenAuthorizationFilter filter = new TokenAuthorizationFilter(tokenService, tokenStore);
        AuthorizationFilterContext context = CreateContext($"Bearer {oldToken}");

        await filter.OnAuthorizationAsync(context);

        AssertUnauthorizedWithReason(context, UnauthorizedReason.DuplicateLogin);
        Assert.Null(context.HttpContext.User.Identity?.Name);
    }

    [Fact]
    public async Task OnAuthorizationAsync_ValidTokenMatchingCache_SetsHttpContextUser()
    {
        ITokenService tokenService = CreateTokenService();
        FakeActiveTokenStore tokenStore = new FakeActiveTokenStore();
        (string token, DateTimeOffset expiresAt) = tokenService.GenerateToken(CreateTestUser());
        tokenStore.SetActiveToken(TestAcc, token, expiresAt);

        TokenAuthorizationFilter filter = new TokenAuthorizationFilter(tokenService, tokenStore);
        AuthorizationFilterContext context = CreateContext($"Bearer {token}");

        await filter.OnAuthorizationAsync(context);

        Assert.Null(context.Result);
        Assert.Equal(TestAcc, context.HttpContext.User.FindFirst(TokenClaimTypes.Acc)?.Value);
    }
}
