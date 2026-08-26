using BoulderingRecordAPI.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace BoulderingRecordAPI.Tests.Controllers;

public class HealthControllerTests
{
    [Fact]
    public void Get_ReturnsOk()
    {
        HealthController controller = new HealthController();

        IActionResult result = controller.Get();

        Assert.IsType<OkResult>(result);
    }
}
