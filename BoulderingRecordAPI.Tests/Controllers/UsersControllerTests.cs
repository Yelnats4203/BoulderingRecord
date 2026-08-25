using System.Security.Claims;
using BoulderingRecordAPI.Controllers;
using BoulderingRecordAPI.Entities;
using BoulderingRecordAPI.Models.Users;
using BoulderingRecordAPI.Tests.Fakes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BoulderingRecordAPI.Tests.Controllers;

public class UsersControllerTests
{
    private static UsersController CreateController(FakeUserRepository userRepository)
    {
        UsersController controller = new UsersController(userRepository, new FakeFriendRequestRepository());
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        return controller;
    }

    private static UsersController CreateAuthenticatedController(FakeUserRepository userRepository, Guid currentUserId)
    {
        UsersController controller = new UsersController(userRepository, new FakeFriendRequestRepository());
        ClaimsIdentity identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, currentUserId.ToString())], "Test");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) },
        };
        return controller;
    }

    [Fact]
    public async Task Create_NewAcc_ReturnsCreatedAndPersistsHashedPassword()
    {
        FakeUserRepository userRepository = new FakeUserRepository([]);
        UsersController controller = CreateController(userRepository);

        IActionResult result = await controller.Create(
            new CreateUserRequest("新使用者", "newacc", "Password123!", true, false),
            CancellationToken.None);

        ObjectResult created = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status201Created, created.StatusCode);
        UserResponse response = Assert.IsType<UserResponse>(created.Value);
        Assert.Equal("新使用者", response.Username);
        Assert.Equal("newacc", response.Acc);
        Assert.True(response.HasEditPermission);

        User? stored = await userRepository.GetByAccAsync("newacc", CancellationToken.None);
        Assert.NotNull(stored);
        Assert.NotEqual("Password123!", stored!.Psw);
    }

    [Fact]
    public async Task Create_IsDemoAccTrue_PersistsDemoAccountFlag()
    {
        FakeUserRepository userRepository = new FakeUserRepository([]);
        UsersController controller = CreateController(userRepository);

        IActionResult result = await controller.Create(
            new CreateUserRequest("測試帳號", "demoacc", "Password123!", false, true),
            CancellationToken.None);

        ObjectResult created = Assert.IsType<ObjectResult>(result);
        UserResponse response = Assert.IsType<UserResponse>(created.Value);
        Assert.True(response.IsDemoAcc);

        User? stored = await userRepository.GetByAccAsync("demoacc", CancellationToken.None);
        Assert.NotNull(stored);
        Assert.True(stored!.IsDemoAcc);
    }

    [Fact]
    public async Task Create_DuplicateAcc_ReturnsBadRequest()
    {
        User existing = new User { Username = "既有使用者", Acc = "dupacc", Psw = "hashed", CreatedAt = DateTime.UtcNow };
        FakeUserRepository userRepository = new FakeUserRepository([existing]);
        UsersController controller = CreateController(userRepository);

        IActionResult result = await controller.Create(
            new CreateUserRequest("新使用者", "dupacc", "Password123!", false, false),
            CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Theory]
    [InlineData("", "acc", "psw")]
    [InlineData("name", "", "psw")]
    [InlineData("name", "acc", "")]
    public async Task Create_MissingRequiredField_ReturnsBadRequest(string username, string acc, string psw)
    {
        UsersController controller = CreateController(new FakeUserRepository([]));

        IActionResult result = await controller.Create(
            new CreateUserRequest(username, acc, psw, false, false), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Theory]
    [InlineData("password123")]
    [InlineData("PASSWORD123!")]
    [InlineData("Password!!!!")]
    [InlineData("Pass1!")]
    public async Task Create_WeakPassword_ReturnsBadRequest(string psw)
    {
        UsersController controller = CreateController(new FakeUserRepository([]));

        IActionResult result = await controller.Create(
            new CreateUserRequest("新使用者", "newacc", psw, false, false), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetAll_ReturnsAllUsersWithoutPassword()
    {
        User first = new User { Username = "使用者一", Acc = "acc1", Psw = "hashed1", HasEditPermission = true, CreatedAt = DateTime.UtcNow };
        User second = new User { Username = "使用者二", Acc = "acc2", Psw = "hashed2", HasEditPermission = false, CreatedAt = DateTime.UtcNow };
        FakeUserRepository userRepository = new FakeUserRepository([first, second]);
        UsersController controller = CreateController(userRepository);

        IActionResult result = await controller.GetAll(CancellationToken.None);

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
        List<UserResponse> responses = Assert.IsAssignableFrom<IEnumerable<UserResponse>>(ok.Value).ToList();
        Assert.Equal(2, responses.Count);
        Assert.Contains(responses, r => r.Acc == "acc1" && r.Username == "使用者一" && r.HasEditPermission);
        Assert.Contains(responses, r => r.Acc == "acc2" && r.Username == "使用者二" && !r.HasEditPermission);
    }

    [Fact]
    public async Task ResetPassword_ExistingAcc_ReturnsNoContentAndPersistsHashedPassword()
    {
        User existing = new User { Username = "既有使用者", Acc = "targetacc", Psw = "old-hash", CreatedAt = DateTime.UtcNow };
        FakeUserRepository userRepository = new FakeUserRepository([existing]);
        UsersController controller = CreateController(userRepository);

        IActionResult result = await controller.ResetPassword(
            new AdminResetPasswordRequest("targetacc", "NewPassword123!"), CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        User? stored = await userRepository.GetByAccAsync("targetacc", CancellationToken.None);
        Assert.NotNull(stored);
        Assert.NotEqual("old-hash", stored!.Psw);
        Assert.NotEqual("NewPassword123!", stored.Psw);
    }

    [Fact]
    public async Task ResetPassword_UnknownAcc_ReturnsNotFound()
    {
        UsersController controller = CreateController(new FakeUserRepository([]));

        IActionResult result = await controller.ResetPassword(
            new AdminResetPasswordRequest("nosuchacc", "NewPassword123!"), CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Theory]
    [InlineData("password123")]
    [InlineData("PASSWORD123!")]
    [InlineData("Pass1!")]
    public async Task ResetPassword_WeakPassword_ReturnsBadRequest(string newPsw)
    {
        User existing = new User { Username = "既有使用者", Acc = "targetacc", Psw = "old-hash", CreatedAt = DateTime.UtcNow };
        UsersController controller = CreateController(new FakeUserRepository([existing]));

        IActionResult result = await controller.ResetPassword(
            new AdminResetPasswordRequest("targetacc", newPsw), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Theory]
    [InlineData("", "psw")]
    [InlineData("acc", "")]
    public async Task ResetPassword_MissingRequiredField_ReturnsBadRequest(string acc, string newPsw)
    {
        UsersController controller = CreateController(new FakeUserRepository([]));

        IActionResult result = await controller.ResetPassword(
            new AdminResetPasswordRequest(acc, newPsw), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Search_CurrentUserWithoutEditPermission_ExcludesAdminCandidates()
    {
        User currentUser = new User { Username = "一般搜尋者", Acc = "searcher", Psw = "hashed", HasEditPermission = false, CreatedAt = DateTime.UtcNow };
        User normalCandidate = new User { Username = "一般攀岩者", Acc = "climber", Psw = "hashed", HasEditPermission = false, CreatedAt = DateTime.UtcNow };
        User adminCandidate = new User { Username = "管理攀岩者", Acc = "admin1", Psw = "hashed", HasEditPermission = true, CreatedAt = DateTime.UtcNow };
        FakeUserRepository userRepository = new FakeUserRepository([currentUser, normalCandidate, adminCandidate]);
        UsersController controller = CreateAuthenticatedController(userRepository, currentUser.Id);

        IActionResult result = await controller.Search("攀岩", CancellationToken.None);

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
        List<UserSearchResponse> responses = Assert.IsAssignableFrom<IEnumerable<UserSearchResponse>>(ok.Value).ToList();
        Assert.Contains(responses, r => r.Id == normalCandidate.Id);
        Assert.DoesNotContain(responses, r => r.Id == adminCandidate.Id);
    }

    [Fact]
    public async Task Search_CurrentUserWithEditPermission_IncludesAllCandidates()
    {
        User currentUser = new User { Username = "管理搜尋者", Acc = "admin-searcher", Psw = "hashed", HasEditPermission = true, CreatedAt = DateTime.UtcNow };
        User normalCandidate = new User { Username = "一般攀岩者", Acc = "climber", Psw = "hashed", HasEditPermission = false, CreatedAt = DateTime.UtcNow };
        User adminCandidate = new User { Username = "管理攀岩者", Acc = "admin1", Psw = "hashed", HasEditPermission = true, CreatedAt = DateTime.UtcNow };
        FakeUserRepository userRepository = new FakeUserRepository([currentUser, normalCandidate, adminCandidate]);
        UsersController controller = CreateAuthenticatedController(userRepository, currentUser.Id);

        IActionResult result = await controller.Search("攀岩", CancellationToken.None);

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
        List<UserSearchResponse> responses = Assert.IsAssignableFrom<IEnumerable<UserSearchResponse>>(ok.Value).ToList();
        Assert.Contains(responses, r => r.Id == normalCandidate.Id);
        Assert.Contains(responses, r => r.Id == adminCandidate.Id);
    }

    [Fact]
    public async Task Search_ExcludesCurrentUserFromResults()
    {
        User currentUser = new User { Username = "攀岩搜尋者", Acc = "searcher", Psw = "hashed", HasEditPermission = false, CreatedAt = DateTime.UtcNow };
        User otherUser = new User { Username = "攀岩夥伴", Acc = "partner", Psw = "hashed", HasEditPermission = false, CreatedAt = DateTime.UtcNow };
        FakeUserRepository userRepository = new FakeUserRepository([currentUser, otherUser]);
        UsersController controller = CreateAuthenticatedController(userRepository, currentUser.Id);

        IActionResult result = await controller.Search("攀岩", CancellationToken.None);

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
        List<UserSearchResponse> responses = Assert.IsAssignableFrom<IEnumerable<UserSearchResponse>>(ok.Value).ToList();
        Assert.Contains(responses, r => r.Id == otherUser.Id);
        Assert.DoesNotContain(responses, r => r.Id == currentUser.Id);
    }
}
