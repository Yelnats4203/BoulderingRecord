using System.Security.Claims;
using BoulderingRecordAPI.Entities;
using BoulderingRecordAPI.Filters;
using BoulderingRecordAPI.Models.Sessions;
using BoulderingRecordAPI.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace BoulderingRecordAPI.Controllers;

/// <summary>
/// 處理抱石活動紀錄的建立、查詢、修改與刪除等端點。
/// </summary>
[ApiController]
[Route("[controller]")]
[TokenAuthorize]
public class SessionsController(ISessionRepository sessionRepository) : ControllerBase
{
    /// <summary>
    /// 建立抱石活動紀錄，所屬使用者由後端指派。
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SessionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] SessionRequest request, CancellationToken cancellationToken)
    {
        Guid? userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        Session session = new Session
        {
            UserId = userId.Value,
            Date = request.Date,
            GymName = request.GymName,
            GradeRecords = request.GradeCounts
                .Select(g => new SessionGradeRecord
                {
                    Grade = g.Grade,
                    CompletedCount = g.CompletedCount,
                    UncompletedCount = g.UncompletedCount,
                })
                .ToList(),
        };

        await sessionRepository.AddAsync(session, cancellationToken);
        await sessionRepository.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = session.Id }, SessionResponse.FromEntity(session));
    }

    /// <summary>
    /// 取得目前使用者的所有抱石活動紀錄清單。
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SessionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        Guid? userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        List<Session> sessions = await sessionRepository.GetAllByUserIdAsync(userId.Value, cancellationToken);
        return Ok(sessions.Select(SessionResponse.FromEntity));
    }

    /// <summary>
    /// 依 ID 取得單筆抱石活動紀錄，不存在或非本人擁有則回傳 404。
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        Guid? userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        Session? session = await sessionRepository.GetByIdAsync(id, cancellationToken);
        if (session is null || session.UserId != userId.Value)
        {
            return NotFound();
        }

        return Ok(SessionResponse.FromEntity(session));
    }

    /// <summary>
    /// 更新抱石活動紀錄的日期、岩館與各級數統計，不存在或非本人擁有則回傳 404。
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(SessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] SessionRequest request, CancellationToken cancellationToken)
    {
        Guid? userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        Session? session = await sessionRepository.GetByIdAsync(id, cancellationToken);
        if (session is null || session.UserId != userId.Value)
        {
            return NotFound();
        }

        session.Date = request.Date;
        session.GymName = request.GymName;
        session.GradeRecords.Clear();
        session.GradeRecords.AddRange(request.GradeCounts.Select(g => new SessionGradeRecord
        {
            Grade = g.Grade,
            CompletedCount = g.CompletedCount,
            UncompletedCount = g.UncompletedCount,
        }));

        await sessionRepository.SaveChangesAsync(cancellationToken);

        return Ok(SessionResponse.FromEntity(session));
    }

    /// <summary>
    /// 刪除抱石活動紀錄，不存在或非本人擁有則回傳 404。
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        Guid? userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        Session? session = await sessionRepository.GetByIdAsync(id, cancellationToken);
        if (session is null || session.UserId != userId.Value)
        {
            return NotFound();
        }

        sessionRepository.Remove(session);
        await sessionRepository.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private Guid? GetUserId()
    {
        string? value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(value, out Guid id) ? id : null;
    }
}
