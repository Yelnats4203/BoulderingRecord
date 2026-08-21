namespace BoulderingRecordAPI.Models.Sends;

/// <summary>
/// 建立完攀紀錄請求，於影片已直接上傳至 Cloudinary 後呼叫。
/// </summary>
/// <param name="SendId">呼叫 upload-authorization 端點時取得的紀錄 ID。</param>
/// <param name="GymName">岩館名稱。</param>
/// <param name="Difficulty">攀岩難度。</param>
/// <param name="Note">備註。</param>
/// <param name="ClimbAt">攀爬日期，未提供時預設為今日。</param>
public record CreateSendRequest(Guid SendId, string? GymName, int? Difficulty, string? Note, DateOnly? ClimbAt = null);
