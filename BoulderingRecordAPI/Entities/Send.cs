namespace BoulderingRecordAPI.Entities;

/// <summary>
/// 完攀紀錄實體。
/// </summary>
public class Send
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
    /// 攀爬日期，前端新增／編輯時可輸入。
    /// </summary>
    public DateOnly ClimbAt { get; set; }

    /// <summary>
    /// 紀錄建立日期，由後端固定寫入，不對前端開放編輯或顯示。
    /// </summary>
    public DateOnly UploadedAt { get; set; }

    /// <summary>
    /// 攀岩難度。
    /// </summary>
    public int? Difficulty { get; set; }

    /// <summary>
    /// 嘗試次數，可為 null；若有值須為正整數。
    /// </summary>
    public int? Attempts { get; set; }

    /// <summary>
    /// 上傳者的使用者 ID。
    /// </summary>
    public Guid UploaderId { get; set; }

    /// <summary>
    /// 影片於 Cloudinary 的 public ID。
    /// </summary>
    public string VideoPublicId { get; set; } = string.Empty;

    /// <summary>
    /// 備註。
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// 紀錄的可見度設定。
    /// </summary>
    public SendVisibility Visibility { get; set; } = SendVisibility.Private;
}
