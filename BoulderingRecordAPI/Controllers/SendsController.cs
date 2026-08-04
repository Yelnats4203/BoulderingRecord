using System.Security.Claims;
using BoulderingRecordAPI.Entities;
using BoulderingRecordAPI.Filters;
using BoulderingRecordAPI.Models.Sends;
using BoulderingRecordAPI.Repositories;
using BoulderingRecordAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

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
    private static readonly FileExtensionContentTypeProvider ContentTypeProvider = new();

    /// <summary>
    /// 上傳完攀紀錄影片與相關資訊，上傳者與上傳時間由後端指派。
    /// </summary>
    [TokenAuthorize]
    [HttpPost]
    [ProducesResponseType(typeof(SendResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Upload([FromForm] UploadSendRequest request, CancellationToken cancellationToken)
    {
        Guid? uploaderId = GetUploaderId();
        if (uploaderId is null)
        {
            return Unauthorized();
        }

        Send send = new Send
        {
            GymName = request.GymName,
            Difficulty = request.Difficulty,
            Note = request.Note,
            UploaderId = uploaderId.Value,
            UploadedAt = DateTimeOffset.UtcNow,
        };

        send.VideoPath = await videoStorageService.SaveAsync(request.Video, uploaderId.Value, send.Id, cancellationToken);

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
    /// 依 ID 讀取紀錄的影片串流；私人紀錄僅上傳者本人可存取。
    /// </summary>
    [TokenAuthorize]
    [HttpGet("{id:guid}/video")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
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

        if (!System.IO.File.Exists(send.VideoPath))
        {
            return NotFound();
        }

        string contentType = ContentTypeProvider.TryGetContentType(send.VideoPath, out string? resolvedContentType)
            ? resolvedContentType
            : "application/octet-stream";

        return PhysicalFile(send.VideoPath, contentType, enableRangeProcessing: true);
    }

    private Guid? GetUploaderId()
    {
        string? value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(value, out Guid id) ? id : null;
    }
}
