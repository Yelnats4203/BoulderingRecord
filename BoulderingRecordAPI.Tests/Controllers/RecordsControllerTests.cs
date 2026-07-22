using BoulderingRecordAPI.Controllers;

namespace BoulderingRecordAPI.Tests.Controllers;

public class RecordsControllerTests
{
    [Fact]
    public void Upload_NotImplementedYet_ThrowsNotImplementedException()
    {
        var controller = new RecordsController();

        Assert.Throws<NotImplementedException>(() => controller.Upload());
    }

    [Fact]
    public void GetAll_NotImplementedYet_ThrowsNotImplementedException()
    {
        var controller = new RecordsController();

        Assert.Throws<NotImplementedException>(() => controller.GetAll());
    }

    [Fact]
    public void GetById_NotImplementedYet_ThrowsNotImplementedException()
    {
        var controller = new RecordsController();

        Assert.Throws<NotImplementedException>(() => controller.GetById(1));
    }
}
