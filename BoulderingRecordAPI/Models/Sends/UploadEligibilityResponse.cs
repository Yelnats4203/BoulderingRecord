namespace BoulderingRecordAPI.Models.Sends;

/// <summary>
/// 上傳資格檢查結果。
/// </summary>
/// <param name="IsAllowed">目前登入使用者是否可繼續上傳影片。</param>
public record UploadEligibilityResponse(bool IsAllowed);
