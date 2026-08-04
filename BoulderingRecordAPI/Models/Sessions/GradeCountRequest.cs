namespace BoulderingRecordAPI.Models.Sessions;

/// <summary>
/// 建立或更新活動紀錄時，單一 V-Scale 級數的攀爬次數統計輸入。
/// </summary>
/// <param name="Grade">V-Scale 難度級數（例如 3 代表 V3）。</param>
/// <param name="CompletedCount">該級數完攀（成功登頂）的路線數。</param>
/// <param name="UncompletedCount">該級數未完攀（嘗試但未成功）的路線數。</param>
public record GradeCountRequest(int Grade, int CompletedCount, int UncompletedCount);
