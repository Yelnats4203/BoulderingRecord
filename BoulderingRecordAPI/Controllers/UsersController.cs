using System.Security.Claims;
using BoulderingRecordAPI.Entities;
using BoulderingRecordAPI.Filters;
using BoulderingRecordAPI.Models.Friends;
using BoulderingRecordAPI.Models.Users;
using BoulderingRecordAPI.Repositories;
using BoulderingRecordAPI.Validation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BoulderingRecordAPI.Controllers;

/// <summary>
/// 處理使用者帳號的建立、搜尋等端點。
/// </summary>
[ApiController]
[Route("[controller]")]
public class UsersController(IUserRepository userRepository, IFriendRequestRepository friendRequestRepository) : ControllerBase
{
    private static readonly PasswordHasher<User> PasswordHasher = new();

    /// <summary>
    /// 建立新的使用者帳號，僅具編輯權限的使用者可呼叫，可同時指定新使用者是否具編輯權限。
    /// </summary>
    [TokenAuthorize]
    [RequireEditPermission]
    [HttpPost]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Acc) ||
            string.IsNullOrWhiteSpace(request.Psw))
        {
            return BadRequest("使用者名稱、帳號、密碼皆為必填。");
        }

        if (!PasswordPolicy.IsValid(request.Psw))
        {
            return BadRequest(PasswordPolicy.ErrorMessage);
        }

        User? existing = await userRepository.GetByAccAsync(request.Acc, cancellationToken);
        if (existing is not null)
        {
            return BadRequest("此帳號已被使用。");
        }

        User user = new User
        {
            Username = request.Username,
            Acc = request.Acc,
            HasEditPermission = request.HasEditPermission,
            IsDemoAcc = request.IsDemoAcc,
            CreatedAt = DateTime.UtcNow,
        };
        user.Psw = PasswordHasher.HashPassword(user, request.Psw);

        await userRepository.AddAsync(user, cancellationToken);
        await userRepository.SaveChangesAsync(cancellationToken);

        return StatusCode(StatusCodes.Status201Created, UserResponse.FromEntity(user));
    }

    /// <summary>
    /// 取得所有使用者清單，僅具編輯權限的使用者可呼叫，回應不含密碼欄位。
    /// </summary>
    [TokenAuthorize]
    [RequireEditPermission]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        List<User> users = await userRepository.GetAllAsync(cancellationToken);
        return Ok(users.Select(UserResponse.FromEntity));
    }

    /// <summary>
    /// 重設指定使用者的密碼，僅具編輯權限的使用者可呼叫，不需驗證該使用者原密碼。
    /// </summary>
    [TokenAuthorize]
    [RequireEditPermission]
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetPassword([FromBody] AdminResetPasswordRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Acc) || string.IsNullOrWhiteSpace(request.NewPsw))
        {
            return BadRequest("帳號與新密碼皆為必填。");
        }

        User? user = await userRepository.GetByAccAsync(request.Acc, cancellationToken);
        if (user is null)
        {
            return NotFound("找不到該帳號。");
        }

        if (!PasswordPolicy.IsValid(request.NewPsw))
        {
            return BadRequest(PasswordPolicy.ErrorMessage);
        }

        user.Psw = PasswordHasher.HashPassword(user, request.NewPsw);
        await userRepository.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// 依使用者名稱模糊搜尋使用者，供好友邀請功能使用；不含帳號、密碼等敏感欄位，並標示與目前登入使用者的好友關係狀態。
    /// </summary>
    [TokenAuthorize]
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<UserSearchResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Search(string? keyword, CancellationToken cancellationToken)
    {
        Guid? currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(keyword))
        {
            return Ok(Array.Empty<UserSearchResponse>());
        }

        User? currentUser = await userRepository.GetByIdAsync(currentUserId.Value, cancellationToken);
        bool excludeEditPermissionUsers = !(currentUser?.HasEditPermission ?? false);

        List<User> candidates = await userRepository.SearchByUsernameAsync(
            keyword, currentUserId.Value, excludeEditPermissionUsers, cancellationToken);
        List<Guid> candidateIds = candidates.Select(u => u.Id).ToList();
        List<FriendRequest> relations = await friendRequestRepository.GetRelationsForUsersAsync(
            currentUserId.Value, candidateIds, cancellationToken);

        List<UserSearchResponse> results = candidates.Select(candidate =>
        {
            FriendRequest? relation = relations.FirstOrDefault(r =>
                r.RequesterId == candidate.Id || r.AddresseeId == candidate.Id);

            FriendRelationStatus status;
            if (relation is null)
            {
                status = FriendRelationStatus.None;
            }
            else if (relation.Status == FriendRequestStatus.Accepted)
            {
                status = FriendRelationStatus.Friends;
            }
            else if (relation.RequesterId == currentUserId.Value)
            {
                status = FriendRelationStatus.RequestSentByMe;
            }
            else
            {
                status = FriendRelationStatus.RequestReceivedFromThem;
            }

            return new UserSearchResponse(candidate.Id, candidate.Username, status, relation?.Id);
        }).ToList();

        return Ok(results);
    }

    private Guid? GetCurrentUserId()
    {
        string? value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(value, out Guid id) ? id : null;
    }
}
