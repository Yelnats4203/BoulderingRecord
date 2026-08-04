using System.Security.Claims;
using BoulderingRecordAPI.Controllers;
using BoulderingRecordAPI.Entities;
using BoulderingRecordAPI.Models.Sends;
using BoulderingRecordAPI.Tests.Fakes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Send = BoulderingRecordAPI.Entities.Send;

namespace BoulderingRecordAPI.Tests.Controllers;

public class SendsControllerTests
{
    private static readonly Guid TestUploaderId = Guid.CreateVersion7();

    private static IFormFile CreateFakeVideo(string fileName = "test.mp4")
    {
        byte[] content = "fake video content"u8.ToArray();
        MemoryStream stream = new MemoryStream(content);
        return new FormFile(stream, 0, stream.Length, "Video", fileName);
    }

    private static SendsController CreateController(
        FakeSendRepository? sendRepository = null,
        bool authenticated = true)
    {
        SendsController controller = new SendsController(
            sendRepository ?? new FakeSendRepository(),
            new FakeVideoStorageService());

        DefaultHttpContext httpContext = new DefaultHttpContext();
        if (authenticated)
        {
            ClaimsIdentity identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, TestUploaderId.ToString())], "Test");
            httpContext.User = new ClaimsPrincipal(identity);
        }

        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        return controller;
    }

    [Fact]
    public async Task Upload_AuthenticatedUser_ReturnsCreatedWithBackendAssignedFields()
    {
        SendsController controller = CreateController();

        IActionResult result = await controller.Upload(
            new UploadSendRequest(CreateFakeVideo(), "測試岩館", 5, "備註"),
            CancellationToken.None);

        CreatedAtActionResult created = Assert.IsType<CreatedAtActionResult>(result);
        SendResponse response = Assert.IsType<SendResponse>(created.Value);
        Assert.Equal("測試岩館", response.GymName);
        Assert.Equal(5, response.Difficulty);
        Assert.Equal("備註", response.Note);
        Assert.Equal(TestUploaderId, response.UploaderId);
        Assert.True(response.UploadedAt <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task Upload_NoAuthenticatedUser_ReturnsUnauthorized()
    {
        SendsController controller = CreateController(authenticated: false);

        IActionResult result = await controller.Upload(
            new UploadSendRequest(CreateFakeVideo(), null, null, null),
            CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task GetAll_ReturnsAllSends()
    {
        Send[] seed = new[]
        {
            new Send { UploaderId = TestUploaderId, UploadedAt = DateTimeOffset.UtcNow, VideoPath = "a.mp4" },
            new Send { UploaderId = TestUploaderId, UploadedAt = DateTimeOffset.UtcNow, VideoPath = "b.mp4" },
        };
        SendsController controller = CreateController(new FakeSendRepository(seed));

        IActionResult result = await controller.GetAll(CancellationToken.None);

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        IEnumerable<SendResponse> sends = Assert.IsAssignableFrom<IEnumerable<SendResponse>>(okResult.Value);
        Assert.Equal(2, sends.Count());
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsSend()
    {
        Send send = new Send { UploaderId = TestUploaderId, UploadedAt = DateTimeOffset.UtcNow, VideoPath = "a.mp4" };
        SendsController controller = CreateController(new FakeSendRepository([send]));

        IActionResult result = await controller.GetById(send.Id, CancellationToken.None);

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        SendResponse response = Assert.IsType<SendResponse>(okResult.Value);
        Assert.Equal(send.Id, response.Id);
    }

    [Fact]
    public async Task GetById_UnknownId_ReturnsNotFound()
    {
        SendsController controller = CreateController();

        IActionResult result = await controller.GetById(Guid.CreateVersion7(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetVideo_UnknownId_ReturnsNotFound()
    {
        SendsController controller = CreateController();

        IActionResult result = await controller.GetVideo(Guid.CreateVersion7(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetVideo_PrivateSend_NotOwner_ReturnsNotFound()
    {
        Send send = new Send
        {
            UploaderId = Guid.CreateVersion7(),
            UploadedAt = DateTimeOffset.UtcNow,
            VideoPath = "someone-else.mp4",
            Visibility = SendVisibility.Private,
        };
        SendsController controller = CreateController(new FakeSendRepository([send]));

        IActionResult result = await controller.GetVideo(send.Id, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetVideo_PrivateSend_Owner_ReturnsPhysicalFile()
    {
        string videoPath = CreateTempVideoFile();
        try
        {
            Send send = new Send
            {
                UploaderId = TestUploaderId,
                UploadedAt = DateTimeOffset.UtcNow,
                VideoPath = videoPath,
                Visibility = SendVisibility.Private,
            };
            SendsController controller = CreateController(new FakeSendRepository([send]));

            IActionResult result = await controller.GetVideo(send.Id, CancellationToken.None);

            PhysicalFileResult fileResult = Assert.IsType<PhysicalFileResult>(result);
            Assert.Equal(videoPath, fileResult.FileName);
            Assert.True(fileResult.EnableRangeProcessing);
        }
        finally
        {
            File.Delete(videoPath);
        }
    }

    [Theory]
    [InlineData(SendVisibility.Public)]
    [InlineData(SendVisibility.Shareable)]
    public async Task GetVideo_PublicOrShareableSend_NotOwner_ReturnsPhysicalFile(SendVisibility visibility)
    {
        string videoPath = CreateTempVideoFile();
        try
        {
            Send send = new Send
            {
                UploaderId = Guid.CreateVersion7(),
                UploadedAt = DateTimeOffset.UtcNow,
                VideoPath = videoPath,
                Visibility = visibility,
            };
            SendsController controller = CreateController(new FakeSendRepository([send]));

            IActionResult result = await controller.GetVideo(send.Id, CancellationToken.None);

            Assert.IsType<PhysicalFileResult>(result);
        }
        finally
        {
            File.Delete(videoPath);
        }
    }

    [Fact]
    public async Task GetVideo_MissingPhysicalFile_ReturnsNotFound()
    {
        Send send = new Send
        {
            UploaderId = Guid.CreateVersion7(),
            UploadedAt = DateTimeOffset.UtcNow,
            VideoPath = Path.Combine(Path.GetTempPath(), $"{Guid.CreateVersion7()}.mp4"),
            Visibility = SendVisibility.Public,
        };
        SendsController controller = CreateController(new FakeSendRepository([send]));

        IActionResult result = await controller.GetVideo(send.Id, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    private static string CreateTempVideoFile()
    {
        string path = Path.Combine(Path.GetTempPath(), $"{Guid.CreateVersion7()}.mp4");
        File.WriteAllBytes(path, "fake video content"u8.ToArray());
        return path;
    }
}
