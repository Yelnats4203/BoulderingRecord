using BoulderingRecordAPI.Entities;
using BoulderingRecordAPI.Filters;
using BoulderingRecordAPI.Models.Users;
using BoulderingRecordAPI.Repositories;
using BoulderingRecordAPI.Validation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BoulderingRecordAPI.Controllers;

/// <summary>
/// 處理使用者帳號的建立等端點。
/// </summary>
[ApiController]
[Route("[controller]")]
public class UsersController(IUserRepository userRepository) : ControllerBase
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
}
