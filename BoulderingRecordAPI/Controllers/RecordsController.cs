using System.Security.Claims;
using BoulderingRecordAPI.Entities;
using BoulderingRecordAPI.Filters;
using BoulderingRecordAPI.Models.Records;
using BoulderingRecordAPI.Repositories;
using BoulderingRecordAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace BoulderingRecordAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecordsController(
    IRecordRepository recordRepository,
    IVideoStorageService videoStorageService) : ControllerBase
{
    [TokenAuthorize]
    [HttpPost]
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

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        List<Record> records = await recordRepository.GetAllAsync(cancellationToken);
        return Ok(records.Select(RecordResponse.FromEntity));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        Record? record = await recordRepository.GetByIdAsync(id, cancellationToken);
        if (record is null)
        {
            return NotFound();
        }

        return Ok(RecordResponse.FromEntity(record));
    }

    private Guid? GetUploaderId()
    {
        string? value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(value, out Guid id) ? id : null;
    }
}
