using BoulderingRecordAPI.Controllers;

namespace BoulderingRecordAPI.Tests.Controllers;

public class AuthControllerTests
{
    [Fact]
    public void Login_NotImplementedYet_ThrowsNotImplementedException()
    {
        var controller = new AuthController();

        Assert.Throws<NotImplementedException>(() => controller.Login());
    }

    [Fact]
    public void Logout_NotImplementedYet_ThrowsNotImplementedException()
    {
        var controller = new AuthController();

        Assert.Throws<NotImplementedException>(() => controller.Logout());
    }
}
