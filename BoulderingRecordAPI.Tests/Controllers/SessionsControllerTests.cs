using System.Security.Claims;
using BoulderingRecordAPI.Controllers;
using BoulderingRecordAPI.Models.Sessions;
using BoulderingRecordAPI.Tests.Fakes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Session = BoulderingRecordAPI.Entities.Session;
using SessionGradeRecord = BoulderingRecordAPI.Entities.SessionGradeRecord;

namespace BoulderingRecordAPI.Tests.Controllers;

public class SessionsControllerTests
{
    private static readonly Guid TestUserId = Guid.CreateVersion7();

    private static SessionsController CreateController(
        FakeSessionRepository? sessionRepository = null,
        bool authenticated = true,
        Guid? userId = null)
    {
        SessionsController controller = new SessionsController(sessionRepository ?? new FakeSessionRepository());

        DefaultHttpContext httpContext = new DefaultHttpContext();
        if (authenticated)
        {
            ClaimsIdentity identity = new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, (userId ?? TestUserId).ToString())], "Test");
            httpContext.User = new ClaimsPrincipal(identity);
        }

        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        return controller;
    }

    private static SessionRequest CreateRequest(
        DateOnly? date = null,
        string? gymName = "測試岩館",
        List<GradeCountRequest>? gradeCounts = null)
        => new SessionRequest(
            date ?? new DateOnly(2026, 8, 4),
            gymName,
            gradeCounts ?? [new GradeCountRequest(3, 2, 1)]);

    [Fact]
    public async Task Create_Authenticated_ReturnsCreatedWithBackendAssignedUserId()
    {
        SessionsController controller = CreateController();

        IActionResult result = await controller.Create(CreateRequest(), CancellationToken.None);

        CreatedAtActionResult created = Assert.IsType<CreatedAtActionResult>(result);
        SessionResponse response = Assert.IsType<SessionResponse>(created.Value);
        Assert.Equal(TestUserId, response.UserId);
        Assert.Equal("測試岩館", response.GymName);
        Assert.Single(response.GradeCounts);
        Assert.Equal(3, response.GradeCounts[0].Grade);
        Assert.Equal(2, response.GradeCounts[0].CompletedCount);
        Assert.Equal(1, response.GradeCounts[0].UncompletedCount);
    }

    [Fact]
    public async Task Create_NotAuthenticated_ReturnsUnauthorized()
    {
        SessionsController controller = CreateController(authenticated: false);

        IActionResult result = await controller.Create(CreateRequest(), CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task GetAll_ReturnsOnlyCurrentUserSessions()
    {
        Guid otherUserId = Guid.CreateVersion7();
        Session[] seed =
        [
            new Session { UserId = TestUserId, Date = new DateOnly(2026, 8, 1) },
            new Session { UserId = TestUserId, Date = new DateOnly(2026, 8, 2) },
            new Session { UserId = otherUserId, Date = new DateOnly(2026, 8, 3) },
        ];
        SessionsController controller = CreateController(new FakeSessionRepository(seed));

        IActionResult result = await controller.GetAll(null, null, CancellationToken.None);

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        IEnumerable<SessionResponse> sessions = Assert.IsAssignableFrom<IEnumerable<SessionResponse>>(okResult.Value);
        Assert.Equal(2, sessions.Count());
        Assert.All(sessions, s => Assert.Equal(TestUserId, s.UserId));
    }

    [Fact]
    public async Task GetAll_DateRangeFilter_ReturnsSessionsWithinRange()
    {
        Session[] seed =
        [
            new Session { UserId = TestUserId, Date = new DateOnly(2026, 8, 1) },
            new Session { UserId = TestUserId, Date = new DateOnly(2026, 8, 5) },
            new Session { UserId = TestUserId, Date = new DateOnly(2026, 8, 10) },
        ];
        SessionsController controller = CreateController(new FakeSessionRepository(seed));

        IActionResult result = await controller.GetAll(new DateOnly(2026, 8, 2), new DateOnly(2026, 8, 9), CancellationToken.None);

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        IEnumerable<SessionResponse> sessions = Assert.IsAssignableFrom<IEnumerable<SessionResponse>>(okResult.Value);
        SessionResponse response = Assert.Single(sessions);
        Assert.Equal(new DateOnly(2026, 8, 5), response.Date);
    }

    [Fact]
    public async Task GetAll_DateRangeFilter_BoundariesAreInclusive()
    {
        Session[] seed =
        [
            new Session { UserId = TestUserId, Date = new DateOnly(2026, 8, 1) },
            new Session { UserId = TestUserId, Date = new DateOnly(2026, 8, 10) },
        ];
        SessionsController controller = CreateController(new FakeSessionRepository(seed));

        IActionResult result = await controller.GetAll(new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 10), CancellationToken.None);

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        IEnumerable<SessionResponse> sessions = Assert.IsAssignableFrom<IEnumerable<SessionResponse>>(okResult.Value);
        Assert.Equal(2, sessions.Count());
    }

    [Fact]
    public async Task GetById_Owner_ReturnsSession()
    {
        Session session = new Session { UserId = TestUserId, Date = new DateOnly(2026, 8, 4) };
        SessionsController controller = CreateController(new FakeSessionRepository([session]));

        IActionResult result = await controller.GetById(session.Id, CancellationToken.None);

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        SessionResponse response = Assert.IsType<SessionResponse>(okResult.Value);
        Assert.Equal(session.Id, response.Id);
    }

    [Fact]
    public async Task GetById_NotOwner_ReturnsNotFound()
    {
        Session session = new Session { UserId = Guid.CreateVersion7(), Date = new DateOnly(2026, 8, 4) };
        SessionsController controller = CreateController(new FakeSessionRepository([session]));

        IActionResult result = await controller.GetById(session.Id, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetById_UnknownId_ReturnsNotFound()
    {
        SessionsController controller = CreateController();

        IActionResult result = await controller.GetById(Guid.CreateVersion7(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Update_Owner_UpdatesFieldsAndGradeCounts()
    {
        Session session = new Session
        {
            UserId = TestUserId,
            Date = new DateOnly(2026, 8, 4),
            GymName = "舊岩館",
            GradeRecords = [new SessionGradeRecord { Grade = 1, CompletedCount = 1, UncompletedCount = 0 }],
        };
        SessionsController controller = CreateController(new FakeSessionRepository([session]));

        IActionResult result = await controller.Update(
            session.Id,
            CreateRequest(date: new DateOnly(2026, 8, 5), gymName: "新岩館", gradeCounts: [new GradeCountRequest(5, 3, 2)]),
            CancellationToken.None);

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        SessionResponse response = Assert.IsType<SessionResponse>(okResult.Value);
        Assert.Equal(new DateOnly(2026, 8, 5), response.Date);
        Assert.Equal("新岩館", response.GymName);
        Assert.Single(response.GradeCounts);
        Assert.Equal(5, response.GradeCounts[0].Grade);
        Assert.Equal(3, response.GradeCounts[0].CompletedCount);
        Assert.Equal(2, response.GradeCounts[0].UncompletedCount);
    }

    [Fact]
    public async Task Update_NotOwner_ReturnsNotFound()
    {
        Session session = new Session { UserId = Guid.CreateVersion7(), Date = new DateOnly(2026, 8, 4) };
        SessionsController controller = CreateController(new FakeSessionRepository([session]));

        IActionResult result = await controller.Update(session.Id, CreateRequest(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_Owner_ReturnsNoContentAndRemoves()
    {
        Session session = new Session { UserId = TestUserId, Date = new DateOnly(2026, 8, 4) };
        FakeSessionRepository repository = new FakeSessionRepository([session]);
        SessionsController controller = CreateController(repository);

        IActionResult result = await controller.Delete(session.Id, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        IActionResult getResult = await controller.GetById(session.Id, CancellationToken.None);
        Assert.IsType<NotFoundResult>(getResult);
    }

    [Fact]
    public async Task Delete_NotOwner_ReturnsNotFound()
    {
        Session session = new Session { UserId = Guid.CreateVersion7(), Date = new DateOnly(2026, 8, 4) };
        SessionsController controller = CreateController(new FakeSessionRepository([session]));

        IActionResult result = await controller.Delete(session.Id, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }
}
