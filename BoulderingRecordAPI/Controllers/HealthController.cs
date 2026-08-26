using Microsoft.AspNetCore.Mvc;

namespace BoulderingRecordAPI.Controllers;

/// <summary>
/// 提供伺服器存活狀態查詢端點，供 UptimeRobot 等外部監控服務定時呼叫確認伺服器是否正常運行。
/// </summary>
[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// 確認伺服器是否正常運行。此端點不需要驗證，供外部監控服務定時呼叫。
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok();
    }
}
