namespace BoulderingRecordAPI.Entities;

/// <summary>
/// 好友邀請／好友關係狀態。
/// </summary>
public enum FriendRequestStatus
{
    /// <summary>
    /// 邀請中，尚未被對方回應。
    /// </summary>
    Pending = 0,

    /// <summary>
    /// 已成為好友。
    /// </summary>
    Accepted = 1,
}
