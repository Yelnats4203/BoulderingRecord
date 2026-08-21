using System.Security.Claims;
using BoulderingRecordAPI.Controllers;
using BoulderingRecordAPI.Entities;
using BoulderingRecordAPI.Models.Sends;
using BoulderingRecordAPI.Repositories;
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
        IUserRepository? userRepository = null,
        bool authenticated = true)
    {
        SendsController controller = new SendsController(
            sendRepository ?? new FakeSendRepository(),
            videoStorageService ?? new FakeVideoStorageService(),
            userRepository ?? new FakeUserRepository([new User { Id = TestUploaderId, IsDemoAcc = false }]));

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

        ObjectResult created = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status201Created, created.StatusCode);
        SendResponse response = Assert.IsType<SendResponse>(created.Value);
        Assert.Equal("測試岩館", response.GymName);
        Assert.Equal(5, response.Difficulty);
        Assert.Equal("備註", response.Note);
        Assert.Equal(TestUploaderId, response.UploaderId);
        Assert.True(response.ClimbAt <= DateOnly.FromDateTime(DateTime.UtcNow));
    }

    [Fact]
    public async Task Upload_NoClimbAtInRequest_DefaultsToToday()
    {
        SendsController controller = CreateController();

        IActionResult result = await controller.Upload(
            new CreateSendRequest(Guid.CreateVersion7(), "測試岩館", 5, "備註"),
            CancellationToken.None);

        ObjectResult created = Assert.IsType<ObjectResult>(result);
        SendResponse response = Assert.IsType<SendResponse>(created.Value);
        Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow), response.ClimbAt);
    }

    [Fact]
    public async Task Upload_ClimbAtInRequest_UsesProvidedDate()
    {
        SendsController controller = CreateController();
        DateOnly providedDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-3);

        IActionResult result = await controller.Upload(
            new CreateSendRequest(Guid.CreateVersion7(), "測試岩館", 5, "備註", providedDate),
            CancellationToken.None);

        ObjectResult created = Assert.IsType<ObjectResult>(result);
        SendResponse response = Assert.IsType<SendResponse>(created.Value);
        Assert.Equal(providedDate, response.ClimbAt);
    }

    [Fact]
    public async Task Upload_AlwaysSetsUploadedAtToTodayRegardlessOfClimbAt()
    {
        FakeSendRepository repository = new FakeSendRepository();
        SendsController controller = CreateController(repository);
        Guid sendId = Guid.CreateVersion7();
        DateOnly pastClimbAt = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-30);

        await controller.Upload(
            new CreateSendRequest(sendId, "測試岩館", 5, "備註", pastClimbAt),
            CancellationToken.None);

        Send? send = await repository.GetByIdAsync(sendId, CancellationToken.None);
        Assert.NotNull(send);
        Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow), send!.UploadedAt);
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
    public async Task Upload_DemoAccountAtDailyLimit_ReturnsBadRequestAndDoesNotCreateRecord()
    {
        User user = new User { Id = TestUploaderId, IsDemoAcc = true };
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        Send[] seed = Enumerable.Range(0, 5)
            .Select(_ => new Send { UploaderId = TestUploaderId, UploadedAt = today, ClimbAt = today, VideoPublicId = Guid.CreateVersion7().ToString() })
            .ToArray();
        FakeSendRepository repository = new FakeSendRepository(seed);
        SendsController controller = CreateController(repository, userRepository: new FakeUserRepository([user]));
        Guid sendId = Guid.CreateVersion7();

        IActionResult result = await controller.Upload(
            new CreateSendRequest(sendId, "測試岩館", 5, "備註"),
            CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
        Send? send = await repository.GetByIdAsync(sendId, CancellationToken.None);
        Assert.Null(send);
    }

    [Fact]
    public async Task Upload_DemoAccountUnderDailyLimit_Succeeds()
    {
        User user = new User { Id = TestUploaderId, IsDemoAcc = true };
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        Send[] seed = Enumerable.Range(0, 4)
            .Select(_ => new Send { UploaderId = TestUploaderId, UploadedAt = today, ClimbAt = today, VideoPublicId = Guid.CreateVersion7().ToString() })
            .ToArray();
        SendsController controller = CreateController(new FakeSendRepository(seed), userRepository: new FakeUserRepository([user]));

        IActionResult result = await controller.Upload(
            new CreateSendRequest(Guid.CreateVersion7(), "測試岩館", 5, "備註"),
            CancellationToken.None);

        ObjectResult created = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status201Created, created.StatusCode);
    }

    [Fact]
    public async Task Upload_NonDemoAccountBeyondFiveUploadsToday_StillSucceeds()
    {
        User user = new User { Id = TestUploaderId, IsDemoAcc = false };
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        Send[] seed = Enumerable.Range(0, 10)
            .Select(_ => new Send { UploaderId = TestUploaderId, UploadedAt = today, ClimbAt = today, VideoPublicId = Guid.CreateVersion7().ToString() })
            .ToArray();
        SendsController controller = CreateController(new FakeSendRepository(seed), userRepository: new FakeUserRepository([user]));

        IActionResult result = await controller.Upload(
            new CreateSendRequest(Guid.CreateVersion7(), "測試岩館", 5, "備註"),
            CancellationToken.None);

        ObjectResult created = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status201Created, created.StatusCode);
    }

    [Fact]
    public async Task GetUploadEligibility_NotAuthenticated_ReturnsUnauthorized()
    {
        SendsController controller = CreateController(authenticated: false);

        IActionResult result = await controller.GetUploadEligibility(CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task GetUploadEligibility_NonDemoAccount_AlwaysAllowed()
    {
        User user = new User { Id = TestUploaderId, IsDemoAcc = false };
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        Send[] seed = Enumerable.Range(0, 10)
            .Select(_ => new Send { UploaderId = TestUploaderId, UploadedAt = today, ClimbAt = today, VideoPublicId = Guid.CreateVersion7().ToString() })
            .ToArray();
        SendsController controller = CreateController(new FakeSendRepository(seed), userRepository: new FakeUserRepository([user]));

        IActionResult result = await controller.GetUploadEligibility(CancellationToken.None);

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        UploadEligibilityResponse response = Assert.IsType<UploadEligibilityResponse>(okResult.Value);
        Assert.True(response.IsAllowed);
    }

    [Fact]
    public async Task GetUploadEligibility_DemoAccountUnderDailyLimit_ReturnsAllowed()
    {
        User user = new User { Id = TestUploaderId, IsDemoAcc = true };
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        Send[] seed = Enumerable.Range(0, 4)
            .Select(_ => new Send { UploaderId = TestUploaderId, UploadedAt = today, ClimbAt = today, VideoPublicId = Guid.CreateVersion7().ToString() })
            .ToArray();
        SendsController controller = CreateController(new FakeSendRepository(seed), userRepository: new FakeUserRepository([user]));

        IActionResult result = await controller.GetUploadEligibility(CancellationToken.None);

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        UploadEligibilityResponse response = Assert.IsType<UploadEligibilityResponse>(okResult.Value);
        Assert.True(response.IsAllowed);
    }

    [Fact]
    public async Task GetUploadEligibility_DemoAccountAtDailyLimit_ReturnsNotAllowed()
    {
        User user = new User { Id = TestUploaderId, IsDemoAcc = true };
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        Send[] seed = Enumerable.Range(0, 5)
            .Select(_ => new Send { UploaderId = TestUploaderId, UploadedAt = today, ClimbAt = today, VideoPublicId = Guid.CreateVersion7().ToString() })
            .ToArray();
        SendsController controller = CreateController(new FakeSendRepository(seed), userRepository: new FakeUserRepository([user]));

        IActionResult result = await controller.GetUploadEligibility(CancellationToken.None);

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        UploadEligibilityResponse response = Assert.IsType<UploadEligibilityResponse>(okResult.Value);
        Assert.False(response.IsAllowed);
    }

    [Fact]
    public async Task GetUploadEligibility_DemoAccountLimitOnlyCountsToday_IgnoresPastUploads()
    {
        User user = new User { Id = TestUploaderId, IsDemoAcc = true };
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        DateOnly yesterday = today.AddDays(-1);
        Send[] seed = Enumerable.Range(0, 10)
            .Select(_ => new Send { UploaderId = TestUploaderId, UploadedAt = yesterday, ClimbAt = yesterday, VideoPublicId = Guid.CreateVersion7().ToString() })
            .ToArray();
        SendsController controller = CreateController(new FakeSendRepository(seed), userRepository: new FakeUserRepository([user]));

        IActionResult result = await controller.GetUploadEligibility(CancellationToken.None);

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        UploadEligibilityResponse response = Assert.IsType<UploadEligibilityResponse>(okResult.Value);
        Assert.True(response.IsAllowed);
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
            new Send { UploaderId = TestUploaderId, ClimbAt = DateOnly.FromDateTime(DateTime.UtcNow), VideoPublicId = "a" },
            new Send { UploaderId = Guid.CreateVersion7(), ClimbAt = DateOnly.FromDateTime(DateTime.UtcNow), VideoPublicId = "b" },
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
            new Send { UploaderId = TestUploaderId, ClimbAt = DateOnly.FromDateTime(DateTime.UtcNow), VideoPublicId = "a", GymName = "True Rock 岩究所" },
            new Send { UploaderId = TestUploaderId, ClimbAt = DateOnly.FromDateTime(DateTime.UtcNow), VideoPublicId = "b", GymName = "彩岩攀岩館" },
        };
        SendsController controller = CreateController(new FakeSendRepository(seed));

        IActionResult result = await controller.GetMine("岩究所", null, null, null, null, CancellationToken.None);

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        IEnumerable<VideoRecordResponse> records = Assert.IsAssignableFrom<IEnumerable<VideoRecordResponse>>(okResult.Value);
        VideoRecordResponse record = Assert.Single(records);
        Assert.Equal(seed[0].Id, record.Id);
    }

    [Fact]
    public async Task GetMine_ClimbAtRangeFilter_ReturnsSendsWithinRange()
    {
        DateOnly now = DateOnly.FromDateTime(DateTime.UtcNow);
        Send[] seed = new[]
        {
            new Send { UploaderId = TestUploaderId, ClimbAt = now.AddDays(-10), VideoPublicId = "a" },
            new Send { UploaderId = TestUploaderId, ClimbAt = now, VideoPublicId = "b" },
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
            new Send { UploaderId = TestUploaderId, ClimbAt = DateOnly.FromDateTime(DateTime.UtcNow), VideoPublicId = "a", Difficulty = 2 },
            new Send { UploaderId = TestUploaderId, ClimbAt = DateOnly.FromDateTime(DateTime.UtcNow), VideoPublicId = "b", Difficulty = 5 },
            new Send { UploaderId = TestUploaderId, ClimbAt = DateOnly.FromDateTime(DateTime.UtcNow), VideoPublicId = "c", Difficulty = 8 },
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
            new Send { UploaderId = TestUploaderId, ClimbAt = DateOnly.FromDateTime(DateTime.UtcNow), VideoPublicId = "a", GymName = "彩岩攀岩館" },
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
            ClimbAt = DateOnly.FromDateTime(DateTime.UtcNow),
            VideoPublicId = "a",
            GymName = "舊岩館",
            Difficulty = 3,
            Note = "舊備註",
        };
        SendsController controller = CreateController(new FakeSendRepository([send]));
        DateOnly newClimbAt = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1);

        IActionResult result = await controller.Update(
            send.Id,
            new UpdateSendRequest(newClimbAt, "新岩館", 7, "新備註"),
            CancellationToken.None);

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        SendResponse response = Assert.IsType<SendResponse>(okResult.Value);
        Assert.Equal(newClimbAt, response.ClimbAt);
        Assert.Equal("新岩館", response.GymName);
        Assert.Equal(7, response.Difficulty);
        Assert.Equal("新備註", response.Note);
    }

    [Fact]
    public async Task Update_DoesNotChangeUploadedAt()
    {
        DateOnly originalUploadedAt = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-5);
        Send send = new Send
        {
            UploaderId = TestUploaderId,
            ClimbAt = DateOnly.FromDateTime(DateTime.UtcNow),
            UploadedAt = originalUploadedAt,
            VideoPublicId = "a",
        };
        FakeSendRepository repository = new FakeSendRepository([send]);
        SendsController controller = CreateController(repository);

        await controller.Update(
            send.Id,
            new UpdateSendRequest(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1), "新岩館", 7, "新備註"),
            CancellationToken.None);

        Send? updated = await repository.GetByIdAsync(send.Id, CancellationToken.None);
        Assert.NotNull(updated);
        Assert.Equal(originalUploadedAt, updated!.UploadedAt);
    }

    [Fact]
    public async Task Update_ClimbAtDefault_ReturnsBadRequest()
    {
        Send send = new Send { UploaderId = TestUploaderId, ClimbAt = DateOnly.FromDateTime(DateTime.UtcNow), VideoPublicId = "a" };
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
        Send send = new Send { UploaderId = Guid.CreateVersion7(), ClimbAt = DateOnly.FromDateTime(DateTime.UtcNow), VideoPublicId = "a" };
        SendsController controller = CreateController(new FakeSendRepository([send]));

        IActionResult result = await controller.Update(
            send.Id,
            new UpdateSendRequest(DateOnly.FromDateTime(DateTime.UtcNow), "新岩館", 7, "新備註"),
            CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Update_NotAuthenticated_ReturnsUnauthorized()
    {
        SendsController controller = CreateController(authenticated: false);

        IActionResult result = await controller.Update(
            Guid.CreateVersion7(),
            new UpdateSendRequest(DateOnly.FromDateTime(DateTime.UtcNow), "新岩館", 7, "新備註"),
            CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Delete_Owner_DeletesRecordAndCloudinaryResource()
    {
        Send send = new Send { UploaderId = TestUploaderId, ClimbAt = DateOnly.FromDateTime(DateTime.UtcNow), VideoPublicId = "sends/owner/video" };
        FakeSendRepository repository = new FakeSendRepository([send]);
        FakeVideoStorageService videoStorageService = new FakeVideoStorageService();
        SendsController controller = CreateController(repository, videoStorageService);

        IActionResult result = await controller.Delete(send.Id, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        Assert.Contains("sends/owner/video", videoStorageService.DeletedPublicIds);
        Send? remaining = await repository.GetByIdAsync(send.Id, CancellationToken.None);
        Assert.Null(remaining);
    }

    [Fact]
    public async Task Delete_NotOwner_ReturnsNotFound()
    {
        Send send = new Send { UploaderId = Guid.CreateVersion7(), ClimbAt = DateOnly.FromDateTime(DateTime.UtcNow), VideoPublicId = "a" };
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
            ClimbAt = DateOnly.FromDateTime(DateTime.UtcNow),
            VideoPublicId = "someone-else",
            Visibility = SendVisibility.Private,
        };
        SendsController controller = CreateController(new FakeSendRepository([send]));

        IActionResult result = await controller.GetVideo(send.Id, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetVideo_PrivateSend_Owner_ReturnsSignedUrl()
    {
        Send send = new Send
        {
            UploaderId = TestUploaderId,
            ClimbAt = DateOnly.FromDateTime(DateTime.UtcNow),
            VideoPublicId = "sends/owner/video",
            Visibility = SendVisibility.Private,
        };
        SendsController controller = CreateController(new FakeSendRepository([send]));

        IActionResult result = await controller.GetVideo(send.Id, CancellationToken.None);

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        VideoPlaybackResponse response = Assert.IsType<VideoPlaybackResponse>(okResult.Value);
        Assert.Equal("https://fake-cdn.test/sends/owner/video?token=fake", response.PlaybackUrl);
    }

    [Theory]
    [InlineData(SendVisibility.Public)]
    [InlineData(SendVisibility.Shareable)]
    public async Task GetVideo_PublicOrShareableSend_NotOwner_ReturnsSignedUrl(SendVisibility visibility)
    {
        Send send = new Send
        {
            UploaderId = Guid.CreateVersion7(),
            ClimbAt = DateOnly.FromDateTime(DateTime.UtcNow),
            VideoPublicId = "sends/someone-else/video",
            Visibility = visibility,
        };
        SendsController controller = CreateController(new FakeSendRepository([send]));

        IActionResult result = await controller.GetVideo(send.Id, CancellationToken.None);

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        VideoPlaybackResponse response = Assert.IsType<VideoPlaybackResponse>(okResult.Value);
        Assert.Equal("https://fake-cdn.test/sends/someone-else/video?token=fake", response.PlaybackUrl);
    }
}
