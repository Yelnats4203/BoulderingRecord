using System.Security.Claims;
using BoulderingRecordAPI.Controllers;
using BoulderingRecordAPI.Entities;
using BoulderingRecordAPI.Models.Auth;
using BoulderingRecordAPI.Options;
using BoulderingRecordAPI.Services;
using BoulderingRecordAPI.Tests.Fakes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BoulderingRecordAPI.Tests.Controllers;

public class AuthControllerTests
{
    private const string TestAcc = "climber01";
    private const string TestPassword = "correct-password";

    private static readonly PasswordHasher<User> PasswordHasher = new();

    private static User CreateTestUser() => new()
    {
        Username = "測試攀岩者",
        Acc = TestAcc,
        Psw = PasswordHasher.HashPassword(null!, TestPassword),
        CreatedAt = DateTime.UtcNow,
    };

    private static ITokenService CreateTokenService() => new TokenService(Microsoft.Extensions.Options.Options.Create(new JwtSettings
    {
        Key = "test-secret-key-for-unit-tests-1234567890",
        Issuer = "TestIssuer",
        Audience = "TestAudience",
        AccessTokenExpiresMinutes = 120,
    }));

    private static AuthController CreateController(User user, ITokenService tokenService, FakeActiveTokenStore tokenStore, string? authenticatedAcc = null)
    {
        AuthController controller = new AuthController(new FakeUserRepository([user]), tokenService, tokenStore);

        DefaultHttpContext httpContext = new DefaultHttpContext();
        if (authenticatedAcc is not null)
        {
            ClaimsIdentity identity = new ClaimsIdentity([new Claim(TokenClaimTypes.Acc, authenticatedAcc)], "Test");
            httpContext.User = new ClaimsPrincipal(identity);
        }

        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        return controller;
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithToken_AndStoresInCache()
    {
        User user = CreateTestUser();
        FakeActiveTokenStore tokenStore = new FakeActiveTokenStore();
        AuthController controller = CreateController(user, CreateTokenService(), tokenStore);

        IActionResult result = await controller.Login(new LoginRequest(TestAcc, TestPassword), CancellationToken.None);

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        LoginResponse response = Assert.IsType<LoginResponse>(okResult.Value);
        Assert.True(tokenStore.TryGetActiveToken(TestAcc, out string? storedToken));
        Assert.Equal(response.Token, storedToken);
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        User user = CreateTestUser();
        AuthController controller = CreateController(user, CreateTokenService(), new FakeActiveTokenStore());

        IActionResult result = await controller.Login(new LoginRequest(TestAcc, "wrong-password"), CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Login_UnknownAcc_ReturnsUnauthorized()
    {
        User user = CreateTestUser();
        AuthController controller = CreateController(user, CreateTokenService(), new FakeActiveTokenStore());

        IActionResult result = await controller.Login(new LoginRequest("no-such-acc", TestPassword), CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Login_SameAccTwice_OldTokenInvalidated_NewTokenActive()
    {
        User user = CreateTestUser();
        FakeActiveTokenStore tokenStore = new FakeActiveTokenStore();
        ITokenService tokenService = CreateTokenService();
        AuthController controller = CreateController(user, tokenService, tokenStore);

        IActionResult firstResult = await controller.Login(new LoginRequest(TestAcc, TestPassword), CancellationToken.None);
        string firstToken = ((LoginResponse)((OkObjectResult)firstResult).Value!).Token;

        IActionResult secondResult = await controller.Login(new LoginRequest(TestAcc, TestPassword), CancellationToken.None);
        string secondToken = ((LoginResponse)((OkObjectResult)secondResult).Value!).Token;

        Assert.True(tokenStore.TryGetActiveToken(TestAcc, out string? activeToken));
        Assert.Equal(secondToken, activeToken);
        Assert.NotEqual(firstToken, activeToken);
    }

    [Fact]
    public void Logout_RemovesActiveToken()
    {
        User user = CreateTestUser();
        FakeActiveTokenStore tokenStore = new FakeActiveTokenStore();
        tokenStore.SetActiveToken(TestAcc, "some-token", DateTimeOffset.UtcNow.AddHours(2));
        AuthController controller = CreateController(user, CreateTokenService(), tokenStore, authenticatedAcc: TestAcc);

        IActionResult result = controller.Logout();

        Assert.IsType<NoContentResult>(result);
        Assert.False(tokenStore.TryGetActiveToken(TestAcc, out _));
    }

    [Fact]
    public async Task Refresh_ReturnsNewToken_OldTokenNoLongerActive()
    {
        User user = CreateTestUser();
        FakeActiveTokenStore tokenStore = new FakeActiveTokenStore();
        const string oldToken = "old-token";
        tokenStore.SetActiveToken(TestAcc, oldToken, DateTimeOffset.UtcNow.AddHours(2));
        AuthController controller = CreateController(user, CreateTokenService(), tokenStore, authenticatedAcc: TestAcc);

        IActionResult result = await controller.Refresh(CancellationToken.None);

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        RefreshTokenResponse response = Assert.IsType<RefreshTokenResponse>(okResult.Value);
        Assert.True(tokenStore.TryGetActiveToken(TestAcc, out string? activeToken));
        Assert.Equal(response.Token, activeToken);
        Assert.NotEqual(oldToken, activeToken);
    }
}
