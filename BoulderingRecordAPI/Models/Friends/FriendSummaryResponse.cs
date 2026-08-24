using BoulderingRecordAPI.Entities;

namespace BoulderingRecordAPI.Models.Friends;

/// <summary>
/// 好友清單項目回應。
/// </summary>
/// <param name="Id">對應的好友關聯資料列 ID，刪除好友時需要帶入此值。</param>
/// <param name="UserId">好友的使用者 ID。</param>
/// <param name="Username">好友的使用者名稱。</param>
/// <param name="FriendsSince">成為好友的時間。</param>
public record FriendSummaryResponse(Guid Id, Guid UserId, string Username, DateTime FriendsSince)
{
    /// <summary>
    /// 由 <see cref="FriendRequest"/> 實體轉換為回應 DTO。
    /// </summary>
    public static FriendSummaryResponse FromEntity(FriendRequest friendRequest, Guid friendUserId, string friendUsername) =>
        new(friendRequest.Id, friendUserId, friendUsername, friendRequest.CreatedAt);
}
