using System.Security.Claims;
using BoulderingRecordAPI.Controllers;
using BoulderingRecordAPI.Tests.Fakes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BoulderingRecordAPI.Tests.Controllers;

public class GymsControllerTests
{
    private static GymsController CreateController(FakeGymRepository gymRepository)
    {
        GymsController controller = new GymsController(gymRepository);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        return controller;
    }

    private static GymsController CreateAuthenticatedController(FakeGymRepository gymRepository, Guid currentUserId)
    {
        GymsController controller = new GymsController(gymRepository);
        ClaimsIdentity identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, currentUserId.ToString())], "Test");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) },
        };
        return controller;
    }

    [Fact]
    public async Task GetNames_NotAuthenticated_ReturnsUnauthorized()
    {
        GymsController controller = CreateController(new FakeGymRepository([]));

        IActionResult result = await controller.GetNames(CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task GetNames_HasGymNames_ReturnsDistinctSortedNames()
    {
        FakeGymRepository gymRepository = new FakeGymRepository(["Beta Gym", "Alpha Gym", "Beta Gym", "", null!]);
        GymsController controller = CreateAuthenticatedController(gymRepository, Guid.NewGuid());

        IActionResult result = await controller.GetNames(CancellationToken.None);

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
        List<string> gymNames = Assert.IsAssignableFrom<IEnumerable<string>>(ok.Value).ToList();
        Assert.Equal(["Alpha Gym", "Beta Gym"], gymNames);
    }

    [Fact]
    public async Task GetNames_NoGymNames_ReturnsEmptyList()
    {
        FakeGymRepository gymRepository = new FakeGymRepository([]);
        GymsController controller = CreateAuthenticatedController(gymRepository, Guid.NewGuid());

        IActionResult result = await controller.GetNames(CancellationToken.None);

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
        List<string> gymNames = Assert.IsAssignableFrom<IEnumerable<string>>(ok.Value).ToList();
        Assert.Empty(gymNames);
    }
}
