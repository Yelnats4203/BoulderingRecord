namespace BoulderingRecordAPI.Models.Sends;

/// <summary>
/// 編輯完攀紀錄請求。
/// </summary>
/// <param name="UploadedAt">影片上傳時間，必填。</param>
/// <param name="GymName">岩館名稱。</param>
/// <param name="Difficulty">攀岩難度。</param>
/// <param name="Note">備註。</param>
public record UpdateSendRequest(DateTimeOffset UploadedAt, string? GymName, int? Difficulty, string? Note);
