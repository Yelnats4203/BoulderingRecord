using System.Security.Claims;
using BoulderingRecordAPI.Entities;
using BoulderingRecordAPI.Filters;
using BoulderingRecordAPI.Models.Records;
using BoulderingRecordAPI.Repositories;
using BoulderingRecordAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace BoulderingRecordAPI.Controllers;

/// <summary>
/// 處理攀岩紀錄的上傳與查詢等端點。
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class RecordsController(
    IRecordRepository recordRepository,
    IVideoStorageService videoStorageService) : ControllerBase
{
    private static readonly FileExtensionContentTypeProvider ContentTypeProvider = new();

    /// <summary>
    /// 上傳攀岩紀錄影片與相關資訊，上傳者與上傳時間由後端指派。
    /// </summary>
    [TokenAuthorize]
    [HttpPost]
    [ProducesResponseType(typeof(RecordResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Upload([FromForm] UploadRecordRequest request, CancellationToken cancellationToken)
    {
        Guid? uploaderId = GetUploaderId();
        if (uploaderId is null)
        {
            return Unauthorized();
        }

        Record record = new Record
        {
            GymName = request.GymName,
            Difficulty = request.Difficulty,
            Note = request.Note,
            UploaderId = uploaderId.Value,
            UploadedAt = DateTimeOffset.UtcNow,
        };

        record.VideoPath = await videoStorageService.SaveAsync(request.Video, uploaderId.Value, record.Id, cancellationToken);

        await recordRepository.AddAsync(record, cancellationToken);
        await recordRepository.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = record.Id }, RecordResponse.FromEntity(record));
    }

    /// <summary>
    /// 取得所有攀岩紀錄清單。
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RecordResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        List<Record> records = await recordRepository.GetAllAsync(cancellationToken);
        return Ok(records.Select(RecordResponse.FromEntity));
    }

    /// <summary>
    /// 依 ID 取得單筆攀岩紀錄，不存在則回傳 404。
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RecordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        Record? record = await recordRepository.GetByIdAsync(id, cancellationToken);
        if (record is null)
        {
            return NotFound();
        }

        return Ok(RecordResponse.FromEntity(record));
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
        Record? record = await recordRepository.GetByIdAsync(id, cancellationToken);
        if (record is null)
        {
            return NotFound();
        }

        Guid? currentUserId = GetUploaderId();
        bool isOwner = currentUserId is not null && record.UploaderId == currentUserId.Value;
        if (record.Visibility == RecordVisibility.Private && !isOwner)
        {
            return NotFound();
        }

        if (!System.IO.File.Exists(record.VideoPath))
        {
            return NotFound();
        }

        string contentType = ContentTypeProvider.TryGetContentType(record.VideoPath, out string? resolvedContentType)
            ? resolvedContentType
            : "application/octet-stream";

        return PhysicalFile(record.VideoPath, contentType, enableRangeProcessing: true);
    }

    private Guid? GetUploaderId()
    {
        string? value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(value, out Guid id) ? id : null;
    }
}
