namespace BoulderingRecordAPI.Entities;

/// <summary>
/// 抱石活動紀錄實體，代表使用者某一次進館攀岩的整體活動紀錄。
/// </summary>
public class Session
{
    /// <summary>
    /// 紀錄唯一識別碼。
    /// </summary>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// 紀錄所屬的使用者 ID。
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 活動日期。
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// 所在岩館名稱。
    /// </summary>
    public string? GymName { get; set; }

    /// <summary>
    /// 各 V-Scale 級數的攀爬次數統計。
    /// </summary>
    public List<SessionGradeRecord> GradeRecords { get; set; } = [];
}
