namespace BoulderingRecordAPI.Entities;

/// <summary>
/// 單一抱石活動中，特定 V-Scale 級數的攀爬次數統計。
/// </summary>
public class SessionGradeRecord
{
    /// <summary>
    /// V-Scale 難度級數（例如 3 代表 V3）。
    /// </summary>
    public int Grade { get; set; }

    /// <summary>
    /// 該級數完攀（成功登頂）的路線數。
    /// </summary>
    public int CompletedCount { get; set; }

    /// <summary>
    /// 該級數未完攀（嘗試但未成功）的路線數。
    /// </summary>
    public int UncompletedCount { get; set; }
}
