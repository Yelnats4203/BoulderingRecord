using BoulderingRecordAPI.Models.Friends;

namespace BoulderingRecordAPI.Models.Users;

/// <summary>
/// 使用者搜尋結果回應，不含帳號、密碼等敏感欄位。
/// </summary>
/// <param name="Id">使用者唯一識別碼。</param>
/// <param name="Username">顯示用的使用者名稱。</param>
/// <param name="RelationStatus">與目前登入使用者的好友關係狀態。</param>
/// <param name="FriendRequestId">對應的好友關聯資料列 ID；關係狀態為 <see cref="FriendRelationStatus.None"/> 時為 null。</param>
public record UserSearchResponse(Guid Id, string Username, FriendRelationStatus RelationStatus, Guid? FriendRequestId);
