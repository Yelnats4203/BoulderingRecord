using System.Security.Claims;
using BoulderingRecordAPI.Filters;
using BoulderingRecordAPI.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace BoulderingRecordAPI.Controllers;

/// <summary>
/// 處理岩館名稱相關的查詢端點。
/// </summary>
[ApiController]
[Route("[controller]")]
public class GymsController(IGymRepository gymRepository) : ControllerBase
{
    /// <summary>
    /// 取得全站 Sends 與 Sessions 中出現過的岩館名稱清單（去重），供前端輸入岩館名稱時做 autocomplete 建議使用。
    /// 此端點刻意不依使用者過濾，僅需登入即可查詢所有使用者曾輸入過的岩館名稱字串（不含使用者身分或其他攀岩紀錄欄位），
    /// 目的是讓使用者沿用社群已使用過的名稱拼寫，避免同一岩館出現多種寫法。
    /// </summary>
    [TokenAuthorize]
    [HttpGet("names")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetNames(CancellationToken cancellationToken)
    {
        Guid? currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        List<string> gymNames = await gymRepository.GetDistinctGymNamesAsync(cancellationToken);
        return Ok(gymNames);
    }

    private Guid? GetCurrentUserId()
    {
        string? value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(value, out Guid id) ? id : null;
    }
}
