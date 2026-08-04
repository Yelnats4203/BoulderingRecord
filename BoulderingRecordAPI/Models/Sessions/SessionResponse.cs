using BoulderingRecordAPI.Entities;

namespace BoulderingRecordAPI.Models.Sessions;

/// <summary>
/// 抱石活動紀錄回應。
/// </summary>
/// <param name="Id">紀錄唯一識別碼。</param>
/// <param name="UserId">紀錄所屬的使用者 ID。</param>
/// <param name="Date">活動日期。</param>
/// <param name="GymName">所在岩館名稱。</param>
/// <param name="GradeCounts">各 V-Scale 級數的攀爬次數統計清單。</param>
public record SessionResponse(
    Guid Id,
    Guid UserId,
    DateOnly Date,
    string? GymName,
    List<GradeCountResponse> GradeCounts)
{
    /// <summary>
    /// 由 <see cref="Session"/> 實體轉換為回應 DTO。
    /// </summary>
    public static SessionResponse FromEntity(Session session) => new(
        session.Id,
        session.UserId,
        session.Date,
        session.GymName,
        session.GradeRecords.Select(GradeCountResponse.FromEntity).ToList());
}
