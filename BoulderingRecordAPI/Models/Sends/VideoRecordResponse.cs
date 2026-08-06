using BoulderingRecordAPI.Services;

namespace BoulderingRecordAPI.Models.Sends;

/// <summary>
/// 影片紀錄清單／詳細內容回應。
/// </summary>
/// <param name="Id">紀錄唯一識別碼。</param>
/// <param name="GymName">岩館名稱。</param>
/// <param name="UploadedAt">影片上傳時間。</param>
/// <param name="Difficulty">攀岩難度。</param>
/// <param name="Note">備註。</param>
/// <param name="ThumbnailUrl">Cloudinary 簽章縮圖網址。</param>
public record VideoRecordResponse(
    Guid Id,
    string? GymName,
    DateTimeOffset UploadedAt,
    int? Difficulty,
    string? Note,
    string ThumbnailUrl)
{
    /// <summary>
    /// 由 <see cref="Entities.Send"/> 實體轉換為回應 DTO，縮圖網址由 <paramref name="videoStorageService"/> 動態簽章產生。
    /// </summary>
    public static VideoRecordResponse FromEntity(Entities.Send send, IVideoStorageService videoStorageService) => new(
        send.Id,
        send.GymName,
        send.UploadedAt,
        send.Difficulty,
        send.Note,
        videoStorageService.GetSignedThumbnailUrl(send.VideoPublicId));
}
