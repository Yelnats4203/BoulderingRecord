using BoulderingRecordAPI.Entities;

namespace BoulderingRecordAPI.Models.Sends;

/// <summary>
/// 完攀紀錄回應。
/// </summary>
/// <param name="Id">紀錄唯一識別碼。</param>
/// <param name="GymName">岩館名稱。</param>
/// <param name="ClimbAt">攀爬日期。</param>
/// <param name="Difficulty">攀岩難度。</param>
/// <param name="Attempts">嘗試次數，可為 null；若有值須為正整數。</param>
/// <param name="UploaderId">上傳者的使用者 ID。</param>
/// <param name="Note">備註。</param>
/// <param name="Visibility">紀錄的可見度設定。</param>
public record SendResponse(
    Guid Id,
    string? GymName,
    DateOnly ClimbAt,
    int? Difficulty,
    int? Attempts,
    Guid UploaderId,
    string? Note,
    SendVisibility Visibility)
{
    /// <summary>
    /// 由 <see cref="Entities.Send"/> 實體轉換為回應 DTO。
    /// </summary>
    public static SendResponse FromEntity(Entities.Send send) => new(
        send.Id,
        send.GymName,
        send.ClimbAt,
        send.Difficulty,
        send.Attempts,
        send.UploaderId,
        send.Note,
        send.Visibility);
}
