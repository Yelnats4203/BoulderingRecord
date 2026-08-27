using System.Security.Claims;
using BoulderingRecordAPI.Entities;
using BoulderingRecordAPI.Filters;
using BoulderingRecordAPI.Models.Sessions;
using BoulderingRecordAPI.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        if (HasDuplicateGrades(request.GradeCounts))
        {
            return BadRequest("同一難度只能輸入一筆。");
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
    /// 取得目前使用者的所有抱石活動紀錄清單，可選擇性帶入起始日期與結束日期篩選區間。
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SessionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(DateOnly? dateFrom, DateOnly? dateTo, CancellationToken cancellationToken)
    {
        Guid? userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        List<Session> sessions = await sessionRepository.GetAllByUserIdAsync(userId.Value, dateFrom, dateTo, cancellationToken);
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
    [ProducesResponseType(StatusCodes.Status409Conflict)]
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

        if (HasDuplicateGrades(request.GradeCounts))
        {
            return BadRequest("同一難度只能輸入一筆。");
        }

        session.Date = request.Date;
        session.GymName = request.GymName;

        // 對 OwnsMany 的 GradeRecords 逐筆就地更新既有項目、僅新增/刪除差額，
        // 避免 Clear() 後整批 AddRange() 造成 EF Core 將新項目誤判為 UPDATE 而非 INSERT，引發 DbUpdateConcurrencyException。
        List<SessionGradeRecord> gradeRecords = session.GradeRecords;
        for (int i = 0; i < request.GradeCounts.Count; i++)
        {
            GradeCountRequest gradeCount = request.GradeCounts[i];
            if (i < gradeRecords.Count)
            {
                gradeRecords[i].Grade = gradeCount.Grade;
                gradeRecords[i].CompletedCount = gradeCount.CompletedCount;
                gradeRecords[i].UncompletedCount = gradeCount.UncompletedCount;
            }
            else
            {
                gradeRecords.Add(new SessionGradeRecord
                {
                    Grade = gradeCount.Grade,
                    CompletedCount = gradeCount.CompletedCount,
                    UncompletedCount = gradeCount.UncompletedCount,
                });
            }
        }
        if (gradeRecords.Count > request.GradeCounts.Count)
        {
            gradeRecords.RemoveRange(request.GradeCounts.Count, gradeRecords.Count - request.GradeCounts.Count);
        }

        try
        {
            await sessionRepository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict("此紀錄已被其他請求同時修改，請重新整理後再試一次。");
        }

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

    private static bool HasDuplicateGrades(List<GradeCountRequest> gradeCounts)
        => gradeCounts.Select(g => g.Grade).Distinct().Count() != gradeCounts.Count;
}
