using System.Security.Claims;
using BoulderingRecordAPI.Controllers;
using BoulderingRecordAPI.Entities;
using BoulderingRecordAPI.Models.Friends;
using BoulderingRecordAPI.Models.Sends;
using BoulderingRecordAPI.Repositories;
using BoulderingRecordAPI.Services;
using BoulderingRecordAPI.Tests.Fakes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Send = BoulderingRecordAPI.Entities.Send;

namespace BoulderingRecordAPI.Tests.Controllers;

public class FriendsControllerTests
{
    private static readonly Guid CurrentUserId = Guid.CreateVersion7();

    private static FriendsController CreateController(
        FakeFriendRequestRepository? friendRequestRepository = null,
        FakeUserRepository? userRepository = null,
        FakeSendRepository? sendRepository = null,
        IVideoStorageService? videoStorageService = null,
        bool authenticated = true)
    {
        FriendsController controller = new FriendsController(
            friendRequestRepository ?? new FakeFriendRequestRepository(),
            userRepository ?? new FakeUserRepository([]),
            sendRepository ?? new FakeSendRepository(),
            videoStorageService ?? new FakeVideoStorageService());

        DefaultHttpContext httpContext = new DefaultHttpContext();
        if (authenticated)
        {
            ClaimsIdentity identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, CurrentUserId.ToString())], "Test");
            httpContext.User = new ClaimsPrincipal(identity);
        }

        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        return controller;
    }

    // GetFriends

    [Fact]
    public async Task GetFriends_NotAuthenticated_ReturnsUnauthorized()
    {
        FriendsController controller = CreateController(authenticated: false);

        IActionResult result = await controller.GetFriends(CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task GetFriends_ReturnsAcceptedRelationsRegardlessOfDirection()
    {
        Guid friendA = Guid.CreateVersion7();
        Guid friendB = Guid.CreateVersion7();
        FriendRequest acceptedAsRequester = new FriendRequest { RequesterId = CurrentUserId, AddresseeId = friendA, Status = FriendRequestStatus.Accepted };
        FriendRequest acceptedAsAddressee = new FriendRequest { RequesterId = friendB, AddresseeId = CurrentUserId, Status = FriendRequestStatus.Accepted };
        FriendRequest pendingUnrelated = new FriendRequest { RequesterId = CurrentUserId, AddresseeId = Guid.CreateVersion7(), Status = FriendRequestStatus.Pending };
        FakeFriendRequestRepository friendRequestRepository = new FakeFriendRequestRepository([acceptedAsRequester, acceptedAsAddressee, pendingUnrelated]);
        FakeUserRepository userRepository = new FakeUserRepository([
            new User { Id = friendA, Username = "好友甲" },
            new User { Id = friendB, Username = "好友乙" },
        ]);
        FriendsController controller = CreateController(friendRequestRepository, userRepository);

        IActionResult result = await controller.GetFriends(CancellationToken.None);

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
        List<FriendSummaryResponse> responses = Assert.IsAssignableFrom<IEnumerable<FriendSummaryResponse>>(ok.Value).ToList();
        Assert.Equal(2, responses.Count);
        Assert.Contains(responses, r => r.UserId == friendA && r.Username == "好友甲");
        Assert.Contains(responses, r => r.UserId == friendB && r.Username == "好友乙");
    }

    // GetPendingRequests

    [Fact]
    public async Task GetPendingRequests_NotAuthenticated_ReturnsUnauthorized()
    {
        FriendsController controller = CreateController(authenticated: false);

        IActionResult result = await controller.GetPendingRequests(CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task GetPendingRequests_OnlyReturnsPendingReceivedByCurrentUser()
    {
        Guid requester = Guid.CreateVersion7();
        FriendRequest receivedPending = new FriendRequest { RequesterId = requester, AddresseeId = CurrentUserId, Status = FriendRequestStatus.Pending };
        FriendRequest sentByMePending = new FriendRequest { RequesterId = CurrentUserId, AddresseeId = Guid.CreateVersion7(), Status = FriendRequestStatus.Pending };
        FriendRequest alreadyAccepted = new FriendRequest { RequesterId = Guid.CreateVersion7(), AddresseeId = CurrentUserId, Status = FriendRequestStatus.Accepted };
        FakeFriendRequestRepository friendRequestRepository = new FakeFriendRequestRepository([receivedPending, sentByMePending, alreadyAccepted]);
        FakeUserRepository userRepository = new FakeUserRepository([new User { Id = requester, Username = "邀請者" }]);
        FriendsController controller = CreateController(friendRequestRepository, userRepository);

        IActionResult result = await controller.GetPendingRequests(CancellationToken.None);

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
        List<FriendRequestResponse> responses = Assert.IsAssignableFrom<IEnumerable<FriendRequestResponse>>(ok.Value).ToList();
        FriendRequestResponse response = Assert.Single(responses);
        Assert.Equal(receivedPending.Id, response.Id);
        Assert.Equal(requester, response.OtherUserId);
        Assert.Equal("邀請者", response.OtherUsername);
    }

    // SendRequest

    [Fact]
    public async Task SendRequest_NotAuthenticated_ReturnsUnauthorized()
    {
        FriendsController controller = CreateController(authenticated: false);

        IActionResult result = await controller.SendRequest(new SendFriendRequestRequest(Guid.CreateVersion7()), CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task SendRequest_ToSelf_ReturnsBadRequest()
    {
        FriendsController controller = CreateController();

        IActionResult result = await controller.SendRequest(new SendFriendRequestRequest(CurrentUserId), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task SendRequest_AddresseeNotFound_ReturnsNotFound()
    {
        FriendsController controller = CreateController();

        IActionResult result = await controller.SendRequest(new SendFriendRequestRequest(Guid.CreateVersion7()), CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task SendRequest_AlreadyPendingBetweenUsers_ReturnsBadRequest()
    {
        Guid addresseeId = Guid.CreateVersion7();
        FriendRequest existing = new FriendRequest { RequesterId = CurrentUserId, AddresseeId = addresseeId, Status = FriendRequestStatus.Pending };
        FakeFriendRequestRepository friendRequestRepository = new FakeFriendRequestRepository([existing]);
        FakeUserRepository userRepository = new FakeUserRepository([new User { Id = addresseeId, Username = "對方" }]);
        FriendsController controller = CreateController(friendRequestRepository, userRepository);

        IActionResult result = await controller.SendRequest(new SendFriendRequestRequest(addresseeId), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task SendRequest_AlreadyFriends_ReturnsBadRequest()
    {
        Guid addresseeId = Guid.CreateVersion7();
        FriendRequest existing = new FriendRequest { RequesterId = CurrentUserId, AddresseeId = addresseeId, Status = FriendRequestStatus.Accepted };
        FakeFriendRequestRepository friendRequestRepository = new FakeFriendRequestRepository([existing]);
        FakeUserRepository userRepository = new FakeUserRepository([new User { Id = addresseeId, Username = "對方" }]);
        FriendsController controller = CreateController(friendRequestRepository, userRepository);

        IActionResult result = await controller.SendRequest(new SendFriendRequestRequest(addresseeId), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task SendRequest_Valid_CreatesPendingRequestAndReturnsCreated()
    {
        Guid addresseeId = Guid.CreateVersion7();
        FakeFriendRequestRepository friendRequestRepository = new FakeFriendRequestRepository();
        FakeUserRepository userRepository = new FakeUserRepository([new User { Id = addresseeId, Username = "對方" }]);
        FriendsController controller = CreateController(friendRequestRepository, userRepository);

        IActionResult result = await controller.SendRequest(new SendFriendRequestRequest(addresseeId), CancellationToken.None);

        ObjectResult created = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status201Created, created.StatusCode);
        FriendRequestResponse response = Assert.IsType<FriendRequestResponse>(created.Value);
        Assert.Equal(addresseeId, response.OtherUserId);
        Assert.Equal("對方", response.OtherUsername);

        FriendRequest? persisted = await friendRequestRepository.GetByIdAsync(response.Id, CancellationToken.None);
        Assert.NotNull(persisted);
        Assert.Equal(FriendRequestStatus.Pending, persisted!.Status);
        Assert.Equal(CurrentUserId, persisted.RequesterId);
    }

    // Accept

    [Fact]
    public async Task Accept_NotAuthenticated_ReturnsUnauthorized()
    {
        FriendsController controller = CreateController(authenticated: false);

        IActionResult result = await controller.Accept(Guid.CreateVersion7(), CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Accept_UnknownId_ReturnsNotFound()
    {
        FriendsController controller = CreateController();

        IActionResult result = await controller.Accept(Guid.CreateVersion7(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Accept_CurrentUserNotAddressee_ReturnsNotFound()
    {
        FriendRequest request = new FriendRequest { RequesterId = Guid.CreateVersion7(), AddresseeId = Guid.CreateVersion7(), Status = FriendRequestStatus.Pending };
        FakeFriendRequestRepository friendRequestRepository = new FakeFriendRequestRepository([request]);
        FriendsController controller = CreateController(friendRequestRepository);

        IActionResult result = await controller.Accept(request.Id, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Accept_AlreadyAccepted_ReturnsNotFound()
    {
        FriendRequest request = new FriendRequest { RequesterId = Guid.CreateVersion7(), AddresseeId = CurrentUserId, Status = FriendRequestStatus.Accepted };
        FakeFriendRequestRepository friendRequestRepository = new FakeFriendRequestRepository([request]);
        FriendsController controller = CreateController(friendRequestRepository);

        IActionResult result = await controller.Accept(request.Id, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Accept_PendingAsAddressee_UpdatesStatusAndReturnsOk()
    {
        Guid requesterId = Guid.CreateVersion7();
        FriendRequest request = new FriendRequest { RequesterId = requesterId, AddresseeId = CurrentUserId, Status = FriendRequestStatus.Pending };
        FakeFriendRequestRepository friendRequestRepository = new FakeFriendRequestRepository([request]);
        FakeUserRepository userRepository = new FakeUserRepository([new User { Id = requesterId, Username = "邀請者" }]);
        FriendsController controller = CreateController(friendRequestRepository, userRepository);

        IActionResult result = await controller.Accept(request.Id, CancellationToken.None);

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
        FriendSummaryResponse response = Assert.IsType<FriendSummaryResponse>(ok.Value);
        Assert.Equal(requesterId, response.UserId);
        Assert.Equal("邀請者", response.Username);

        FriendRequest? updated = await friendRequestRepository.GetByIdAsync(request.Id, CancellationToken.None);
        Assert.NotNull(updated);
        Assert.Equal(FriendRequestStatus.Accepted, updated!.Status);
    }

    // Delete

    [Fact]
    public async Task Delete_NotAuthenticated_ReturnsUnauthorized()
    {
        FriendsController controller = CreateController(authenticated: false);

        IActionResult result = await controller.Delete(Guid.CreateVersion7(), CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Delete_UnknownId_ReturnsNotFound()
    {
        FriendsController controller = CreateController();

        IActionResult result = await controller.Delete(Guid.CreateVersion7(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_CurrentUserNotPartOfRelation_ReturnsNotFound()
    {
        FriendRequest request = new FriendRequest { RequesterId = Guid.CreateVersion7(), AddresseeId = Guid.CreateVersion7(), Status = FriendRequestStatus.Pending };
        FakeFriendRequestRepository friendRequestRepository = new FakeFriendRequestRepository([request]);
        FriendsController controller = CreateController(friendRequestRepository);

        IActionResult result = await controller.Delete(request.Id, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_AsRequester_RemovesRequestAndReturnsNoContent()
    {
        FriendRequest request = new FriendRequest { RequesterId = CurrentUserId, AddresseeId = Guid.CreateVersion7(), Status = FriendRequestStatus.Pending };
        FakeFriendRequestRepository friendRequestRepository = new FakeFriendRequestRepository([request]);
        FriendsController controller = CreateController(friendRequestRepository);

        IActionResult result = await controller.Delete(request.Id, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        FriendRequest? remaining = await friendRequestRepository.GetByIdAsync(request.Id, CancellationToken.None);
        Assert.Null(remaining);
    }

    [Fact]
    public async Task Delete_AsAddressee_RemovesRequestAndReturnsNoContent()
    {
        FriendRequest request = new FriendRequest { RequesterId = Guid.CreateVersion7(), AddresseeId = CurrentUserId, Status = FriendRequestStatus.Accepted };
        FakeFriendRequestRepository friendRequestRepository = new FakeFriendRequestRepository([request]);
        FriendsController controller = CreateController(friendRequestRepository);

        IActionResult result = await controller.Delete(request.Id, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        FriendRequest? remaining = await friendRequestRepository.GetByIdAsync(request.Id, CancellationToken.None);
        Assert.Null(remaining);
    }

    // GetFriendVideos

    [Fact]
    public async Task GetFriendVideos_NotAuthenticated_ReturnsUnauthorized()
    {
        FriendsController controller = CreateController(authenticated: false);

        IActionResult result = await controller.GetFriendVideos(Guid.CreateVersion7(), CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task GetFriendVideos_NoRelation_ReturnsNotFound()
    {
        FriendsController controller = CreateController();

        IActionResult result = await controller.GetFriendVideos(Guid.CreateVersion7(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetFriendVideos_PendingNotAccepted_ReturnsNotFound()
    {
        Guid targetUserId = Guid.CreateVersion7();
        FriendRequest request = new FriendRequest { RequesterId = CurrentUserId, AddresseeId = targetUserId, Status = FriendRequestStatus.Pending };
        FakeFriendRequestRepository friendRequestRepository = new FakeFriendRequestRepository([request]);
        FriendsController controller = CreateController(friendRequestRepository);

        IActionResult result = await controller.GetFriendVideos(targetUserId, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetFriendVideos_Accepted_ReturnsOnlyPublicSends()
    {
        Guid targetUserId = Guid.CreateVersion7();
        FriendRequest request = new FriendRequest { RequesterId = CurrentUserId, AddresseeId = targetUserId, Status = FriendRequestStatus.Accepted };
        FakeFriendRequestRepository friendRequestRepository = new FakeFriendRequestRepository([request]);
        Send publicSend = new Send { UploaderId = targetUserId, ClimbAt = DateOnly.FromDateTime(DateTime.UtcNow), VideoPublicId = "public", Visibility = SendVisibility.Public };
        Send privateSend = new Send { UploaderId = targetUserId, ClimbAt = DateOnly.FromDateTime(DateTime.UtcNow), VideoPublicId = "private", Visibility = SendVisibility.Private };
        FakeSendRepository sendRepository = new FakeSendRepository([publicSend, privateSend]);
        FriendsController controller = CreateController(friendRequestRepository, sendRepository: sendRepository);

        IActionResult result = await controller.GetFriendVideos(targetUserId, CancellationToken.None);

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
        List<VideoRecordResponse> responses = Assert.IsAssignableFrom<IEnumerable<VideoRecordResponse>>(ok.Value).ToList();
        VideoRecordResponse response = Assert.Single(responses);
        Assert.Equal(publicSend.Id, response.Id);
    }

    // GetRecentVideos

    [Fact]
    public async Task GetRecentVideos_NotAuthenticated_ReturnsUnauthorized()
    {
        FriendsController controller = CreateController(authenticated: false);

        IActionResult result = await controller.GetRecentVideos(CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task GetRecentVideos_MergesPublicSendsAcrossFriendsOrderedByUploadedAtDescending()
    {
        Guid friendA = Guid.CreateVersion7();
        Guid friendB = Guid.CreateVersion7();
        Guid strangerId = Guid.CreateVersion7();
        FriendRequest relationA = new FriendRequest { RequesterId = CurrentUserId, AddresseeId = friendA, Status = FriendRequestStatus.Accepted };
        FriendRequest relationB = new FriendRequest { RequesterId = friendB, AddresseeId = CurrentUserId, Status = FriendRequestStatus.Accepted };
        FakeFriendRequestRepository friendRequestRepository = new FakeFriendRequestRepository([relationA, relationB]);
        FakeUserRepository userRepository = new FakeUserRepository([
            new User { Id = friendA, Username = "好友甲" },
            new User { Id = friendB, Username = "好友乙" },
        ]);
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        Send olderFromA = new Send { UploaderId = friendA, ClimbAt = today, UploadedAt = today.AddDays(-2), VideoPublicId = "older", Visibility = SendVisibility.Public };
        Send newerFromB = new Send { UploaderId = friendB, ClimbAt = today, UploadedAt = today, VideoPublicId = "newer", Visibility = SendVisibility.Public };
        Send privateFromA = new Send { UploaderId = friendA, ClimbAt = today, UploadedAt = today, VideoPublicId = "private", Visibility = SendVisibility.Private };
        Send fromStranger = new Send { UploaderId = strangerId, ClimbAt = today, UploadedAt = today, VideoPublicId = "stranger", Visibility = SendVisibility.Public };
        FakeSendRepository sendRepository = new FakeSendRepository([olderFromA, newerFromB, privateFromA, fromStranger]);
        FriendsController controller = CreateController(friendRequestRepository, userRepository, sendRepository);

        IActionResult result = await controller.GetRecentVideos(CancellationToken.None);

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
        List<FriendVideoResponse> responses = Assert.IsAssignableFrom<IEnumerable<FriendVideoResponse>>(ok.Value).ToList();
        Assert.Equal(2, responses.Count);
        Assert.Equal(newerFromB.Id, responses[0].Video.Id);
        Assert.Equal("好友乙", responses[0].FriendUsername);
        Assert.Equal(olderFromA.Id, responses[1].Video.Id);
        Assert.Equal("好友甲", responses[1].FriendUsername);
    }
}
