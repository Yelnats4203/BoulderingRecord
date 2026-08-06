using System.Security.Claims;
using BoulderingRecordAPI.Controllers;
using BoulderingRecordAPI.Entities;
using BoulderingRecordAPI.Models.Sends;
using BoulderingRecordAPI.Services;
using BoulderingRecordAPI.Tests.Fakes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Send = BoulderingRecordAPI.Entities.Send;

namespace BoulderingRecordAPI.Tests.Controllers;

public class SendsControllerTests
{
    private static readonly Guid TestUploaderId = Guid.CreateVersion7();

    private static SendsController CreateController(
        FakeSendRepository? sendRepository = null,
        IVideoStorageService? videoStorageService = null,
        bool authenticated = true)
    {
        SendsController controller = new SendsController(
            sendRepository ?? new FakeSendRepository(),
            videoStorageService ?? new FakeVideoStorageService());

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
    public void UploadAuthorization_Authenticated_ReturnsAuthorizationForUser()
    {
        SendsController controller = CreateController();

        IActionResult result = controller.GetUploadAuthorization();

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        UploadAuthorizationResponse response = Assert.IsType<UploadAuthorizationResponse>(okResult.Value);
        Assert.Contains(TestUploaderId.ToString(), response.PublicId);
        Assert.Contains(TestUploaderId.ToString(), response.Folder);
    }

    [Fact]
    public void UploadAuthorization_NotAuthenticated_ReturnsUnauthorized()
    {
        SendsController controller = CreateController(authenticated: false);

        IActionResult result = controller.GetUploadAuthorization();

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Upload_AuthenticatedUser_ReturnsCreatedWithBackendAssignedFields()
    {
        SendsController controller = CreateController();

        IActionResult result = await controller.Upload(
            new CreateSendRequest(Guid.CreateVersion7(), "測試岩館", 5, "備註"),
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
            new CreateSendRequest(Guid.CreateVersion7(), null, null, null),
            CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Upload_ResourceNotUploaded_ReturnsBadRequest()
    {
        SendsController controller = CreateController(videoStorageService: new FakeVideoStorageService(resourceExists: false));

        IActionResult result = await controller.Upload(
            new CreateSendRequest(Guid.CreateVersion7(), "測試岩館", 5, "備註"),
            CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetAll_ReturnsAllSends()
    {
        Send[] seed = new[]
        {
            new Send { UploaderId = TestUploaderId, UploadedAt = DateTimeOffset.UtcNow, VideoPublicId = "a" },
            new Send { UploaderId = TestUploaderId, UploadedAt = DateTimeOffset.UtcNow, VideoPublicId = "b" },
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
        Send send = new Send { UploaderId = TestUploaderId, UploadedAt = DateTimeOffset.UtcNow, VideoPublicId = "a" };
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
    public async Task GetMine_NotAuthenticated_ReturnsUnauthorized()
    {
        SendsController controller = CreateController(authenticated: false);

        IActionResult result = await controller.GetMine(null, null, null, null, null, CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task GetMine_NoFilter_ReturnsOnlyOwnSends()
    {
        Send[] seed = new[]
        {
            new Send { UploaderId = TestUploaderId, UploadedAt = DateTimeOffset.UtcNow, VideoPublicId = "a" },
            new Send { UploaderId = Guid.CreateVersion7(), UploadedAt = DateTimeOffset.UtcNow, VideoPublicId = "b" },
        };
        SendsController controller = CreateController(new FakeSendRepository(seed));

        IActionResult result = await controller.GetMine(null, null, null, null, null, CancellationToken.None);

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        IEnumerable<VideoRecordResponse> records = Assert.IsAssignableFrom<IEnumerable<VideoRecordResponse>>(okResult.Value);
        VideoRecordResponse record = Assert.Single(records);
        Assert.Equal(seed[0].Id, record.Id);
        Assert.Equal("https://fake-cdn.test/a.jpg?token=fake", record.ThumbnailUrl);
    }

    [Fact]
    public async Task GetMine_GymNameFilter_ReturnsPartialMatchOnly()
    {
        Send[] seed = new[]
        {
            new Send { UploaderId = TestUploaderId, UploadedAt = DateTimeOffset.UtcNow, VideoPublicId = "a", GymName = "True Rock 岩究所" },
            new Send { UploaderId = TestUploaderId, UploadedAt = DateTimeOffset.UtcNow, VideoPublicId = "b", GymName = "彩岩攀岩館" },
        };
        SendsController controller = CreateController(new FakeSendRepository(seed));

        IActionResult result = await controller.GetMine("岩究所", null, null, null, null, CancellationToken.None);

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        IEnumerable<VideoRecordResponse> records = Assert.IsAssignableFrom<IEnumerable<VideoRecordResponse>>(okResult.Value);
        VideoRecordResponse record = Assert.Single(records);
        Assert.Equal(seed[0].Id, record.Id);
    }

    [Fact]
    public async Task GetMine_UploadedAtRangeFilter_ReturnsSendsWithinRange()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        Send[] seed = new[]
        {
            new Send { UploaderId = TestUploaderId, UploadedAt = now.AddDays(-10), VideoPublicId = "a" },
            new Send { UploaderId = TestUploaderId, UploadedAt = now, VideoPublicId = "b" },
        };
        SendsController controller = CreateController(new FakeSendRepository(seed));

        IActionResult result = await controller.GetMine(null, now.AddDays(-1), now.AddDays(1), null, null, CancellationToken.None);

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        IEnumerable<VideoRecordResponse> records = Assert.IsAssignableFrom<IEnumerable<VideoRecordResponse>>(okResult.Value);
        VideoRecordResponse record = Assert.Single(records);
        Assert.Equal(seed[1].Id, record.Id);
    }

    [Fact]
    public async Task GetMine_DifficultyRangeFilter_ReturnsSendsWithinRange()
    {
        Send[] seed = new[]
        {
            new Send { UploaderId = TestUploaderId, UploadedAt = DateTimeOffset.UtcNow, VideoPublicId = "a", Difficulty = 2 },
            new Send { UploaderId = TestUploaderId, UploadedAt = DateTimeOffset.UtcNow, VideoPublicId = "b", Difficulty = 5 },
            new Send { UploaderId = TestUploaderId, UploadedAt = DateTimeOffset.UtcNow, VideoPublicId = "c", Difficulty = 8 },
        };
        SendsController controller = CreateController(new FakeSendRepository(seed));

        IActionResult result = await controller.GetMine(null, null, null, 4, 6, CancellationToken.None);

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        IEnumerable<VideoRecordResponse> records = Assert.IsAssignableFrom<IEnumerable<VideoRecordResponse>>(okResult.Value);
        VideoRecordResponse record = Assert.Single(records);
        Assert.Equal(seed[1].Id, record.Id);
    }

    [Fact]
    public async Task GetMine_NoMatchingSends_ReturnsEmpty()
    {
        Send[] seed = new[]
        {
            new Send { UploaderId = TestUploaderId, UploadedAt = DateTimeOffset.UtcNow, VideoPublicId = "a", GymName = "彩岩攀岩館" },
        };
        SendsController controller = CreateController(new FakeSendRepository(seed));

        IActionResult result = await controller.GetMine("不存在的岩館", null, null, null, null, CancellationToken.None);

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        IEnumerable<VideoRecordResponse> records = Assert.IsAssignableFrom<IEnumerable<VideoRecordResponse>>(okResult.Value);
        Assert.Empty(records);
    }

    [Fact]
    public async Task Update_Owner_UpdatesFields()
    {
        Send send = new Send
        {
            UploaderId = TestUploaderId,
            UploadedAt = DateTimeOffset.UtcNow,
            VideoPublicId = "a",
            GymName = "舊岩館",
            Difficulty = 3,
            Note = "舊備註",
        };
        SendsController controller = CreateController(new FakeSendRepository([send]));
        DateTimeOffset newUploadedAt = DateTimeOffset.UtcNow.AddDays(-1);

        IActionResult result = await controller.Update(
            send.Id,
            new UpdateSendRequest(newUploadedAt, "新岩館", 7, "新備註"),
            CancellationToken.None);

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        SendResponse response = Assert.IsType<SendResponse>(okResult.Value);
        Assert.Equal(newUploadedAt, response.UploadedAt);
        Assert.Equal("新岩館", response.GymName);
        Assert.Equal(7, response.Difficulty);
        Assert.Equal("新備註", response.Note);
    }

    [Fact]
    public async Task Update_UploadedAtDefault_ReturnsBadRequest()
    {
        Send send = new Send { UploaderId = TestUploaderId, UploadedAt = DateTimeOffset.UtcNow, VideoPublicId = "a" };
        SendsController controller = CreateController(new FakeSendRepository([send]));

        IActionResult result = await controller.Update(
            send.Id,
            new UpdateSendRequest(default, "新岩館", 7, "新備註"),
            CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Update_NotOwner_ReturnsNotFound()
    {
        Send send = new Send { UploaderId = Guid.CreateVersion7(), UploadedAt = DateTimeOffset.UtcNow, VideoPublicId = "a" };
        SendsController controller = CreateController(new FakeSendRepository([send]));

        IActionResult result = await controller.Update(
            send.Id,
            new UpdateSendRequest(DateTimeOffset.UtcNow, "新岩館", 7, "新備註"),
            CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Update_NotAuthenticated_ReturnsUnauthorized()
    {
        SendsController controller = CreateController(authenticated: false);

        IActionResult result = await controller.Update(
            Guid.CreateVersion7(),
            new UpdateSendRequest(DateTimeOffset.UtcNow, "新岩館", 7, "新備註"),
            CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Delete_Owner_DeletesRecordAndCloudinaryResource()
    {
        Send send = new Send { UploaderId = TestUploaderId, UploadedAt = DateTimeOffset.UtcNow, VideoPublicId = "sends/owner/video" };
        FakeSendRepository repository = new FakeSendRepository([send]);
        FakeVideoStorageService videoStorageService = new FakeVideoStorageService();
        SendsController controller = CreateController(repository, videoStorageService);

        IActionResult result = await controller.Delete(send.Id, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        Assert.Contains("sends/owner/video", videoStorageService.DeletedPublicIds);
        IActionResult getResult = await controller.GetById(send.Id, CancellationToken.None);
        Assert.IsType<NotFoundResult>(getResult);
    }

    [Fact]
    public async Task Delete_NotOwner_ReturnsNotFound()
    {
        Send send = new Send { UploaderId = Guid.CreateVersion7(), UploadedAt = DateTimeOffset.UtcNow, VideoPublicId = "a" };
        SendsController controller = CreateController(new FakeSendRepository([send]));

        IActionResult result = await controller.Delete(send.Id, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_NotAuthenticated_ReturnsUnauthorized()
    {
        SendsController controller = CreateController(authenticated: false);

        IActionResult result = await controller.Delete(Guid.CreateVersion7(), CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result);
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
            VideoPublicId = "someone-else",
            Visibility = SendVisibility.Private,
        };
        SendsController controller = CreateController(new FakeSendRepository([send]));

        IActionResult result = await controller.GetVideo(send.Id, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetVideo_PrivateSend_Owner_ReturnsRedirectToSignedUrl()
    {
        Send send = new Send
        {
            UploaderId = TestUploaderId,
            UploadedAt = DateTimeOffset.UtcNow,
            VideoPublicId = "sends/owner/video",
            Visibility = SendVisibility.Private,
        };
        SendsController controller = CreateController(new FakeSendRepository([send]));

        IActionResult result = await controller.GetVideo(send.Id, CancellationToken.None);

        RedirectResult redirectResult = Assert.IsType<RedirectResult>(result);
        Assert.Equal("https://fake-cdn.test/sends/owner/video?token=fake", redirectResult.Url);
    }

    [Theory]
    [InlineData(SendVisibility.Public)]
    [InlineData(SendVisibility.Shareable)]
    public async Task GetVideo_PublicOrShareableSend_NotOwner_ReturnsRedirectToSignedUrl(SendVisibility visibility)
    {
        Send send = new Send
        {
            UploaderId = Guid.CreateVersion7(),
            UploadedAt = DateTimeOffset.UtcNow,
            VideoPublicId = "sends/someone-else/video",
            Visibility = visibility,
        };
        SendsController controller = CreateController(new FakeSendRepository([send]));

        IActionResult result = await controller.GetVideo(send.Id, CancellationToken.None);

        RedirectResult redirectResult = Assert.IsType<RedirectResult>(result);
        Assert.Equal("https://fake-cdn.test/sends/someone-else/video?token=fake", redirectResult.Url);
    }
}
