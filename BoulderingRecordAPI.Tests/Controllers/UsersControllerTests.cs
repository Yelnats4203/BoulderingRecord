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
        UsersController controller = new UsersController(userRepository);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        return controller;
    }

    [Fact]
    public async Task Create_NewAcc_ReturnsCreatedAndPersistsHashedPassword()
    {
        FakeUserRepository userRepository = new FakeUserRepository([]);
        UsersController controller = CreateController(userRepository);

        IActionResult result = await controller.Create(
            new CreateUserRequest("新使用者", "newacc", "password123", true),
            CancellationToken.None);

        ObjectResult created = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status201Created, created.StatusCode);
        UserResponse response = Assert.IsType<UserResponse>(created.Value);
        Assert.Equal("新使用者", response.Username);
        Assert.Equal("newacc", response.Acc);
        Assert.True(response.HasEditPermission);

        User? stored = await userRepository.GetByAccAsync("newacc", CancellationToken.None);
        Assert.NotNull(stored);
        Assert.NotEqual("password123", stored!.Psw);
    }

    [Fact]
    public async Task Create_DuplicateAcc_ReturnsBadRequest()
    {
        User existing = new User { Username = "既有使用者", Acc = "dupacc", Psw = "hashed", CreatedAt = DateTime.UtcNow };
        FakeUserRepository userRepository = new FakeUserRepository([existing]);
        UsersController controller = CreateController(userRepository);

        IActionResult result = await controller.Create(
            new CreateUserRequest("新使用者", "dupacc", "password123", false),
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
            new CreateUserRequest(username, acc, psw, false), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }
}
