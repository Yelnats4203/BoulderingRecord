namespace BoulderingRecordAPI.Models.Friends;

/// <summary>
/// 送出好友邀請請求。
/// </summary>
/// <param name="AddresseeId">欲送出邀請的目標使用者 ID。</param>
public record SendFriendRequestRequest(Guid AddresseeId);
