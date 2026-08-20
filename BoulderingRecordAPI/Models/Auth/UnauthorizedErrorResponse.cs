namespace BoulderingRecordAPI.Models.Auth;

/// <summary>
/// 未授權回應內容。
/// </summary>
/// <param name="Reason">未授權的原因，供前端顯示對應的提示訊息。</param>
public record UnauthorizedErrorResponse(UnauthorizedReason Reason);
