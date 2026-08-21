namespace BoulderingRecordAPI.Models.Sends;

/// <summary>
/// 編輯完攀紀錄請求。
/// </summary>
/// <param name="ClimbAt">攀爬日期，必填。</param>
/// <param name="GymName">岩館名稱。</param>
/// <param name="Difficulty">攀岩難度。</param>
/// <param name="Note">備註。</param>
public record UpdateSendRequest(DateOnly ClimbAt, string? GymName, int? Difficulty, string? Note);
