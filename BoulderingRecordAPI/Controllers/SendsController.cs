using System.Security.Claims;
using BoulderingRecordAPI.Entities;
using BoulderingRecordAPI.Filters;
using BoulderingRecordAPI.Models.Sends;
using BoulderingRecordAPI.Repositories;
using BoulderingRecordAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace BoulderingRecordAPI.Controllers;

/// <summary>
/// 處理完攀紀錄的上傳與查詢等端點。
/// </summary>
[ApiController]
[Route("[controller]")]
public class SendsController(
    ISendRepository sendRepository,
    IVideoStorageService videoStorageService,
    IUserRepository userRepository) : ControllerBase
{
    /// <summary>
    /// 取得供前端直接上傳影片到 Cloudinary 的簽章授權，上傳完成後需以回傳的 <c>SendId</c> 呼叫 <see cref="Upload"/> 建立紀錄。
    /// </summary>
    [TokenAuthorize]
    [HttpPost("upload-authorization")]
    [ProducesResponseType(typeof(UploadAuthorizationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetUploadAuthorization()
    {
        Guid? uploaderId = GetUploaderId();
        if (uploaderId is null)
        {
            return Unauthorized();
        }

        VideoUploadAuthorization authorization = videoStorageService.CreateUploadAuthorization(uploaderId.Value);
        return Ok(UploadAuthorizationResponse.FromAuthorization(authorization));
    }

    /// <summary>
    /// 確認目前登入使用者是否可繼續上傳影片；僅測試帳號（<see cref="User.IsDemoAcc"/>）受當日上傳筆數限制。
    /// 這個端點僅供前端在壓縮影片前提早提示使用者，實際限制由 <see cref="Upload"/> 端點強制執行。
    /// </summary>
    [TokenAuthorize]
    [HttpGet("upload-eligibility")]
    [ProducesResponseType(typeof(UploadEligibilityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUploadEligibility(CancellationToken cancellationToken)
    {
        Guid? uploaderId = GetUploaderId();
        if (uploaderId is null)
        {
            return Unauthorized();
        }

        User? user = await userRepository.GetByIdAsync(uploaderId.Value, cancellationToken);
        if (user is null)
        {
            return Unauthorized();
        }

        bool isAllowed = await IsUploadAllowedAsync(user, cancellationToken);
        return Ok(new UploadEligibilityResponse(isAllowed));
    }

    /// <summary>
    /// 建立完攀紀錄，須於影片已直接上傳至 Cloudinary 後呼叫；上傳者由後端指派，攀爬日期若未提供則預設為今日，上傳日期一律為今日。
    /// 測試帳號（<see cref="User.IsDemoAcc"/>）當日已達上傳筆數上限時會被拒絕，此限制在此端點強制執行，不只依賴前端的 <see cref="GetUploadEligibility"/> 檢查。
    /// </summary>
    [TokenAuthorize]
    [HttpPost]
    [ProducesResponseType(typeof(SendResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Upload([FromBody] CreateSendRequest request, CancellationToken cancellationToken)
    {
        Guid? uploaderId = GetUploaderId();
        if (uploaderId is null)
        {
            return Unauthorized();
        }

        User? uploader = await userRepository.GetByIdAsync(uploaderId.Value, cancellationToken);
        if (uploader is null)
        {
            return Unauthorized();
        }

        if (!await IsUploadAllowedAsync(uploader, cancellationToken))
        {
            return BadRequest("測試帳號一日僅能上傳5筆。");
        }

        if (request.Attempts is int attempts && attempts < 1)
        {
            return BadRequest("嘗試次數須為正整數。");
        }

        // Cloudinary 上傳時另帶有 folder 參數，實際儲存位置會是 folder 與 public_id 相接後的路徑。
        string publicId = $"Bouldering/{uploaderId.Value}/sends/{uploaderId.Value}/{request.SendId}";
        bool resourceExists = await videoStorageService.ResourceExistsAsync(publicId, cancellationToken);
        if (!resourceExists)
        {
            return BadRequest("找不到對應的已上傳影片。");
        }

        Send send = new Send
        {
            Id = request.SendId,
            GymName = request.GymName,
            Difficulty = request.Difficulty,
            Attempts = request.Attempts,
            Note = request.Note,
            UploaderId = uploaderId.Value,
            ClimbAt = request.ClimbAt ?? DateOnly.FromDateTime(DateTime.UtcNow),
            UploadedAt = DateOnly.FromDateTime(DateTime.UtcNow),
            VideoPublicId = publicId,
            Visibility = request.IsPublic ? SendVisibility.Public : SendVisibility.Private,
        };

        await sendRepository.AddAsync(send, cancellationToken);
        await sendRepository.SaveChangesAsync(cancellationToken);

        return StatusCode(StatusCodes.Status201Created, SendResponse.FromEntity(send));
    }

    /// <summary>
    /// 依岩館名稱、攀爬日期區間、難度區間，取得目前登入使用者自己上傳的影片紀錄清單。
    /// </summary>
    [TokenAuthorize]
    [HttpGet("mine")]
    [ProducesResponseType(typeof(IEnumerable<VideoRecordResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMine(
        string? gymName,
        DateOnly? climbAtFrom,
        DateOnly? climbAtTo,
        int? minDifficulty,
        int? maxDifficulty,
        CancellationToken cancellationToken)
    {
        Guid? uploaderId = GetUploaderId();
        if (uploaderId is null)
        {
            return Unauthorized();
        }

        List<Send> sends = await sendRepository.GetByUploaderIdAsync(
            uploaderId.Value, gymName, climbAtFrom, climbAtTo, minDifficulty, maxDifficulty, cancellationToken);
        return Ok(sends.Select(s => VideoRecordResponse.FromEntity(s, videoStorageService)));
    }

    /// <summary>
    /// 編輯完攀紀錄的攀爬日期、岩館、難度、嘗試次數、備註；僅上傳者本人可編輯，攀爬日期為必填。
    /// </summary>
    [TokenAuthorize]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(SendResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSendRequest request, CancellationToken cancellationToken)
    {
        Guid? uploaderId = GetUploaderId();
        if (uploaderId is null)
        {
            return Unauthorized();
        }

        if (request.ClimbAt == default)
        {
            return BadRequest("攀爬日期為必填。");
        }

        if (request.Attempts is int attempts && attempts < 1)
        {
            return BadRequest("嘗試次數須為正整數。");
        }

        Send? send = await sendRepository.GetByIdAsync(id, cancellationToken);
        if (send is null || send.UploaderId != uploaderId.Value)
        {
            return NotFound();
        }

        send.ClimbAt = request.ClimbAt;
        send.GymName = request.GymName;
        send.Difficulty = request.Difficulty;
        send.Attempts = request.Attempts;
        send.Note = request.Note;

        await sendRepository.SaveChangesAsync(cancellationToken);

        return Ok(SendResponse.FromEntity(send));
    }

    /// <summary>
    /// 刪除完攀紀錄，同時刪除 Cloudinary 上的影片資源；僅上傳者本人可刪除。
    /// </summary>
    [TokenAuthorize]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        Guid? uploaderId = GetUploaderId();
        if (uploaderId is null)
        {
            return Unauthorized();
        }

        Send? send = await sendRepository.GetByIdAsync(id, cancellationToken);
        if (send is null || send.UploaderId != uploaderId.Value)
        {
            return NotFound();
        }

        await videoStorageService.DeleteResourceAsync(send.VideoPublicId, cancellationToken);
        await sendRepository.DeleteAsync(send, cancellationToken);
        await sendRepository.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// 依 ID 取得紀錄影片的時效性簽章播放網址；私人紀錄僅上傳者本人可存取。
    /// </summary>
    [TokenAuthorize]
    [HttpGet("{id:guid}/video")]
    [ProducesResponseType(typeof(VideoPlaybackResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVideo(Guid id, CancellationToken cancellationToken)
    {
        Send? send = await sendRepository.GetByIdAsync(id, cancellationToken);
        if (send is null)
        {
            return NotFound();
        }

        Guid? currentUserId = GetUploaderId();
        bool isOwner = currentUserId is not null && send.UploaderId == currentUserId.Value;
        if (send.Visibility == SendVisibility.Private && !isOwner)
        {
            return NotFound();
        }

        return Ok(new VideoPlaybackResponse(videoStorageService.GetSignedPlaybackUrl(send.VideoPublicId)));
    }

    private Guid? GetUploaderId()
    {
        string? value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(value, out Guid id) ? id : null;
    }

    private async Task<bool> IsUploadAllowedAsync(User user, CancellationToken cancellationToken)
    {
        if (!user.IsDemoAcc)
        {
            return true;
        }

        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        int todayCount = await sendRepository.CountByUploaderIdAndUploadedDateAsync(user.Id, today, cancellationToken);
        return todayCount < 5;
    }
}
