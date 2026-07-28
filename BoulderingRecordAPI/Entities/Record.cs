namespace BoulderingRecordAPI.Entities;

/// <summary>
/// 攀岩紀錄實體。
/// </summary>
public class Record
{
    /// <summary>
    /// 紀錄唯一識別碼。
    /// </summary>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// 岩館名稱。
    /// </summary>
    public string? GymName { get; set; }

    /// <summary>
    /// 影片上傳時間。
    /// </summary>
    public DateTimeOffset UploadedAt { get; set; }

    /// <summary>
    /// 攀岩難度。
    /// </summary>
    public int? Difficulty { get; set; }

    /// <summary>
    /// 上傳者的使用者 ID。
    /// </summary>
    public Guid UploaderId { get; set; }

    /// <summary>
    /// 影片檔案於儲存體中的實際路徑。
    /// </summary>
    public string VideoPath { get; set; } = string.Empty;

    /// <summary>
    /// 備註。
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// 紀錄的可見度設定。
    /// </summary>
    public RecordVisibility Visibility { get; set; } = RecordVisibility.Private;
}
