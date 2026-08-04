namespace BoulderingRecordAPI.Models.Sessions;

/// <summary>
/// 建立或更新抱石活動紀錄請求。
/// </summary>
/// <param name="Date">活動日期。</param>
/// <param name="GymName">所在岩館名稱。</param>
/// <param name="GradeCounts">各 V-Scale 級數的攀爬次數統計清單。</param>
public record SessionRequest(DateOnly Date, string? GymName, List<GradeCountRequest> GradeCounts);
