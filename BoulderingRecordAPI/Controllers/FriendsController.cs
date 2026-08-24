using System.Security.Claims;
using BoulderingRecordAPI.Entities;
using BoulderingRecordAPI.Filters;
using BoulderingRecordAPI.Models.Friends;
using BoulderingRecordAPI.Models.Sends;
using BoulderingRecordAPI.Repositories;
using BoulderingRecordAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace BoulderingRecordAPI.Controllers;

/// <summary>
/// 處理好友邀請、好友清單與好友影片查詢等端點。
/// </summary>
[ApiController]
[Route("[controller]")]
public class FriendsController(
    IFriendRequestRepository friendRequestRepository,
    IUserRepository userRepository,
    ISendRepository sendRepository,
    IVideoStorageService videoStorageService) : ControllerBase
{
    /// <summary>
    /// 取得目前登入使用者的好友清單。
    /// </summary>
    [TokenAuthorize]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FriendSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetFriends(CancellationToken cancellationToken)
    {
        Guid? currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        List<FriendRequest> accepted = await friendRequestRepository.GetAcceptedByUserIdAsync(currentUserId.Value, cancellationToken);
        Dictionary<Guid, string> usernames = await GetUsernameLookupAsync(
            accepted.Select(f => GetOtherUserId(f, currentUserId.Value)), cancellationToken);

        List<FriendSummaryResponse> results = accepted.Select(f =>
        {
            Guid friendUserId = GetOtherUserId(f, currentUserId.Value);
            return FriendSummaryResponse.FromEntity(f, friendUserId, usernames.GetValueOrDefault(friendUserId, string.Empty));
        }).ToList();

        return Ok(results);
    }

    /// <summary>
    /// 取得目前登入使用者收到、尚未回應的好友邀請清單。
    /// </summary>
    [TokenAuthorize]
    [HttpGet("requests")]
    [ProducesResponseType(typeof(IEnumerable<FriendRequestResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPendingRequests(CancellationToken cancellationToken)
    {
        Guid? currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        List<FriendRequest> pending = await friendRequestRepository.GetPendingReceivedAsync(currentUserId.Value, cancellationToken);
        Dictionary<Guid, string> usernames = await GetUsernameLookupAsync(pending.Select(f => f.RequesterId), cancellationToken);

        List<FriendRequestResponse> results = pending
            .Select(f => new FriendRequestResponse(f.Id, f.RequesterId, usernames.GetValueOrDefault(f.RequesterId, string.Empty), f.CreatedAt))
            .ToList();

        return Ok(results);
    }

    /// <summary>
    /// 送出好友邀請；不能對自己送出邀請，且兩人之間若已存在邀請中或已成立的好友關係則會被拒絕。
    /// </summary>
    [TokenAuthorize]
    [HttpPost("requests")]
    [ProducesResponseType(typeof(FriendRequestResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendRequest([FromBody] SendFriendRequestRequest request, CancellationToken cancellationToken)
    {
        Guid? currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        if (request.AddresseeId == currentUserId.Value)
        {
            return BadRequest("不能將自己加為好友。");
        }

        User? addressee = await userRepository.GetByIdAsync(request.AddresseeId, cancellationToken);
        if (addressee is null)
        {
            return NotFound();
        }

        FriendRequest? existing = await friendRequestRepository.GetBetweenUsersAsync(
            currentUserId.Value, request.AddresseeId, cancellationToken);
        if (existing is not null)
        {
            return BadRequest(existing.Status == FriendRequestStatus.Accepted ? "已經是好友。" : "已存在邀請中的好友邀請。");
        }

        FriendRequest friendRequest = new FriendRequest
        {
            RequesterId = currentUserId.Value,
            AddresseeId = request.AddresseeId,
            Status = FriendRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow,
        };

        await friendRequestRepository.AddAsync(friendRequest, cancellationToken);
        await friendRequestRepository.SaveChangesAsync(cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            new FriendRequestResponse(friendRequest.Id, addressee.Id, addressee.Username, friendRequest.CreatedAt));
    }

    /// <summary>
    /// 接受收到的好友邀請，僅該邀請的收件人可呼叫。
    /// </summary>
    [TokenAuthorize]
    [HttpPost("{id:guid}/accept")]
    [ProducesResponseType(typeof(FriendSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Accept(Guid id, CancellationToken cancellationToken)
    {
        Guid? currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        FriendRequest? friendRequest = await friendRequestRepository.GetByIdAsync(id, cancellationToken);
        if (friendRequest is null || friendRequest.AddresseeId != currentUserId.Value || friendRequest.Status != FriendRequestStatus.Pending)
        {
            return NotFound();
        }

        friendRequest.Status = FriendRequestStatus.Accepted;
        await friendRequestRepository.SaveChangesAsync(cancellationToken);

        User? requester = await userRepository.GetByIdAsync(friendRequest.RequesterId, cancellationToken);
        string requesterUsername = requester?.Username ?? string.Empty;

        return Ok(FriendSummaryResponse.FromEntity(friendRequest, friendRequest.RequesterId, requesterUsername));
    }

    /// <summary>
    /// 取消已送出的邀請、拒絕收到的邀請，或刪除已成立的好友關係；僅邀請/好友關係的雙方之一可呼叫。
    /// </summary>
    [TokenAuthorize]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        Guid? currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        FriendRequest? friendRequest = await friendRequestRepository.GetByIdAsync(id, cancellationToken);
        if (friendRequest is null ||
            (friendRequest.RequesterId != currentUserId.Value && friendRequest.AddresseeId != currentUserId.Value))
        {
            return NotFound();
        }

        await friendRequestRepository.DeleteAsync(friendRequest, cancellationToken);
        await friendRequestRepository.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// 取得指定好友的公開影片清單；僅能查詢已成立好友關係的對象。
    /// </summary>
    [TokenAuthorize]
    [HttpGet("{userId:guid}/videos")]
    [ProducesResponseType(typeof(IEnumerable<VideoRecordResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFriendVideos(Guid userId, CancellationToken cancellationToken)
    {
        Guid? currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        FriendRequest? relation = await friendRequestRepository.GetBetweenUsersAsync(currentUserId.Value, userId, cancellationToken);
        if (relation is null || relation.Status != FriendRequestStatus.Accepted)
        {
            return NotFound();
        }

        List<Send> sends = await sendRepository.GetPublicByUploaderIdAsync(userId, cancellationToken);
        return Ok(sends.Select(s => VideoRecordResponse.FromEntity(s, videoStorageService)));
    }

    /// <summary>
    /// 取得所有好友合併後最新 5 筆公開影片，供 Dashboard 的好友動態使用。
    /// </summary>
    [TokenAuthorize]
    [HttpGet("videos/recent")]
    [ProducesResponseType(typeof(IEnumerable<FriendVideoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetRecentVideos(CancellationToken cancellationToken)
    {
        Guid? currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        List<FriendRequest> accepted = await friendRequestRepository.GetAcceptedByUserIdAsync(currentUserId.Value, cancellationToken);
        List<Guid> friendIds = accepted.Select(f => GetOtherUserId(f, currentUserId.Value)).Distinct().ToList();

        List<Send> sends = await sendRepository.GetRecentPublicByUploaderIdsAsync(friendIds, 5, cancellationToken);
        Dictionary<Guid, string> usernames = await GetUsernameLookupAsync(sends.Select(s => s.UploaderId), cancellationToken);

        List<FriendVideoResponse> results = sends
            .Select(s => FriendVideoResponse.FromEntity(s, usernames.GetValueOrDefault(s.UploaderId, string.Empty), videoStorageService))
            .ToList();

        return Ok(results);
    }

    private static Guid GetOtherUserId(FriendRequest friendRequest, Guid currentUserId) =>
        friendRequest.RequesterId == currentUserId ? friendRequest.AddresseeId : friendRequest.RequesterId;

    private async Task<Dictionary<Guid, string>> GetUsernameLookupAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken)
    {
        List<Guid> distinctIds = userIds.Distinct().ToList();
        List<User> users = await userRepository.GetByIdsAsync(distinctIds, cancellationToken);
        return users.ToDictionary(u => u.Id, u => u.Username);
    }

    private Guid? GetCurrentUserId()
    {
        string? value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(value, out Guid id) ? id : null;
    }
}
