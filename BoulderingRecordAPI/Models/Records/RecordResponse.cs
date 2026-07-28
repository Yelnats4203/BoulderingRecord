using BoulderingRecordAPI.Entities;

namespace BoulderingRecordAPI.Models.Records;

/// <summary>
/// 攀岩紀錄回應。
/// </summary>
/// <param name="Id">紀錄唯一識別碼。</param>
/// <param name="GymName">岩館名稱。</param>
/// <param name="UploadedAt">影片上傳時間。</param>
/// <param name="Difficulty">攀岩難度。</param>
/// <param name="UploaderId">上傳者的使用者 ID。</param>
/// <param name="Note">備註。</param>
/// <param name="Visibility">紀錄的可見度設定。</param>
public record RecordResponse(
    Guid Id,
    string? GymName,
    DateTimeOffset UploadedAt,
    int? Difficulty,
    Guid UploaderId,
    string? Note,
    RecordVisibility Visibility)
{
    /// <summary>
    /// 由 <see cref="Entities.Record"/> 實體轉換為回應 DTO。
    /// </summary>
    public static RecordResponse FromEntity(Entities.Record record) => new(
        record.Id,
        record.GymName,
        record.UploadedAt,
        record.Difficulty,
        record.UploaderId,
        record.Note,
        record.Visibility);
}
