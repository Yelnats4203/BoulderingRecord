using Microsoft.AspNetCore.Http;

namespace BoulderingRecordAPI.Models.Sends;

/// <summary>
/// 上傳完攀紀錄請求。
/// </summary>
/// <param name="Video">攀岩影片檔案。</param>
/// <param name="GymName">岩館名稱。</param>
/// <param name="Difficulty">攀岩難度。</param>
/// <param name="Note">備註。</param>
public record UploadSendRequest(IFormFile Video, string? GymName, int? Difficulty, string? Note);
