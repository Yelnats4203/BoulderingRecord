using System.Security.Claims;
using BoulderingRecordAPI.Controllers;
using BoulderingRecordAPI.Models.Records;
using BoulderingRecordAPI.Tests.Fakes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Record = BoulderingRecordAPI.Entities.Record;

namespace BoulderingRecordAPI.Tests.Controllers;

public class RecordsControllerTests
{
    private static readonly Guid TestUploaderId = Guid.CreateVersion7();

    private static IFormFile CreateFakeVideo(string fileName = "test.mp4")
    {
        var content = "fake video content"u8.ToArray();
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, stream.Length, "Video", fileName);
    }

    private static RecordsController CreateController(
        FakeRecordRepository? recordRepository = null,
        bool authenticated = true)
    {
        var controller = new RecordsController(
            recordRepository ?? new FakeRecordRepository(),
            new FakeVideoStorageService());

        var httpContext = new DefaultHttpContext();
        if (authenticated)
        {
            var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, TestUploaderId.ToString())], "Test");
            httpContext.User = new ClaimsPrincipal(identity);
        }

        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        return controller;
    }

    [Fact]
    public async Task Upload_AuthenticatedUser_ReturnsCreatedWithBackendAssignedFields()
    {
        var controller = CreateController();

        var result = await controller.Upload(
            new UploadRecordRequest(CreateFakeVideo(), "測試岩館", 5, "備註"),
            CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        var response = Assert.IsType<RecordResponse>(created.Value);
        Assert.Equal("測試岩館", response.GymName);
        Assert.Equal(5, response.Difficulty);
        Assert.Equal("備註", response.Note);
        Assert.Equal(TestUploaderId, response.UploaderId);
        Assert.True(response.UploadedAt <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task Upload_NoAuthenticatedUser_ReturnsUnauthorized()
    {
        var controller = CreateController(authenticated: false);

        var result = await controller.Upload(
            new UploadRecordRequest(CreateFakeVideo(), null, null, null),
            CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task GetAll_ReturnsAllRecords()
    {
        var seed = new[]
        {
            new Record { UploaderId = TestUploaderId, UploadedAt = DateTimeOffset.UtcNow, VideoPath = "a.mp4" },
            new Record { UploaderId = TestUploaderId, UploadedAt = DateTimeOffset.UtcNow, VideoPath = "b.mp4" },
        };
        var controller = CreateController(new FakeRecordRepository(seed));

        var result = await controller.GetAll(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var records = Assert.IsAssignableFrom<IEnumerable<RecordResponse>>(okResult.Value);
        Assert.Equal(2, records.Count());
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsRecord()
    {
        var record = new Record { UploaderId = TestUploaderId, UploadedAt = DateTimeOffset.UtcNow, VideoPath = "a.mp4" };
        var controller = CreateController(new FakeRecordRepository([record]));

        var result = await controller.GetById(record.Id, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<RecordResponse>(okResult.Value);
        Assert.Equal(record.Id, response.Id);
    }

    [Fact]
    public async Task GetById_UnknownId_ReturnsNotFound()
    {
        var controller = CreateController();

        var result = await controller.GetById(Guid.CreateVersion7(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }
}
