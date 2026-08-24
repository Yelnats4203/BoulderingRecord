namespace BoulderingRecordAPI.Models.Friends;

/// <summary>
/// 好友邀請回應，通用於「送出邀請」與「收到的待處理邀請清單」。
/// </summary>
/// <param name="Id">好友邀請資料列 ID。</param>
/// <param name="OtherUserId">邀請對方的使用者 ID（依情境為邀請發送者或接收者）。</param>
/// <param name="OtherUsername">邀請對方的使用者名稱。</param>
/// <param name="CreatedAt">邀請建立時間。</param>
public record FriendRequestResponse(Guid Id, Guid OtherUserId, string OtherUsername, DateTime CreatedAt);
