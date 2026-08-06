namespace BoulderingRecordAPI.Services;

/// <summary>
/// 供前端直接上傳影片到 Cloudinary 所需的簽章授權資訊。
/// </summary>
/// <param name="SendId">預先產生的完攀紀錄 ID，上傳完成後建立紀錄時需回傳。</param>
/// <param name="PublicId">影片於 Cloudinary 的 public ID。</param>
/// <param name="Folder">影片於 Cloudinary Console 中歸類的資料夾路徑。</param>
/// <param name="CloudName">Cloudinary 雲端名稱。</param>
/// <param name="ApiKey">Cloudinary API Key。</param>
/// <param name="Timestamp">簽章時使用的 Unix 時間戳記。</param>
/// <param name="Signature">依上傳參數計算出的簽章。</param>
public record VideoUploadAuthorization(
    Guid SendId,
    string PublicId,
    string Folder,
    string CloudName,
    string ApiKey,
    long Timestamp,
    string Signature);
