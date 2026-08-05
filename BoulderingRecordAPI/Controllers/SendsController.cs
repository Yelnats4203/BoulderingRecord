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
    IVideoStorageService videoStorageService) : ControllerBase
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
    /// 建立完攀紀錄，須於影片已直接上傳至 Cloudinary 後呼叫；上傳者與上傳時間由後端指派。
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

        string publicId = $"sends/{uploaderId.Value}/{request.SendId}";
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
            Note = request.Note,
            UploaderId = uploaderId.Value,
            UploadedAt = DateTimeOffset.UtcNow,
            VideoPublicId = publicId,
        };

        await sendRepository.AddAsync(send, cancellationToken);
        await sendRepository.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = send.Id }, SendResponse.FromEntity(send));
    }

    /// <summary>
    /// 取得所有完攀紀錄清單。
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SendResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        List<Send> sends = await sendRepository.GetAllAsync(cancellationToken);
        return Ok(sends.Select(SendResponse.FromEntity));
    }

    /// <summary>
    /// 依 ID 取得單筆完攀紀錄，不存在則回傳 404。
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SendResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        Send? send = await sendRepository.GetByIdAsync(id, cancellationToken);
        if (send is null)
        {
            return NotFound();
        }

        return Ok(SendResponse.FromEntity(send));
    }

    /// <summary>
    /// 依 ID 取得紀錄影片的時效性簽章網址並導向播放；私人紀錄僅上傳者本人可存取。
    /// </summary>
    [TokenAuthorize]
    [HttpGet("{id:guid}/video")]
    [ProducesResponseType(StatusCodes.Status302Found)]
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

        return Redirect(videoStorageService.GetSignedPlaybackUrl(send.VideoPublicId));
    }

    private Guid? GetUploaderId()
    {
        string? value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(value, out Guid id) ? id : null;
    }
}
