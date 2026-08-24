namespace BoulderingRecordAPI.Models.Friends;

/// <summary>
/// 使用者搜尋結果與目前登入使用者之間的好友關係狀態。
/// </summary>
public enum FriendRelationStatus
{
    /// <summary>
    /// 尚無任何好友關係或邀請。
    /// </summary>
    None,

    /// <summary>
    /// 目前登入使用者已送出邀請，對方尚未回應。
    /// </summary>
    RequestSentByMe,

    /// <summary>
    /// 對方已送出邀請，目前登入使用者尚未回應。
    /// </summary>
    RequestReceivedFromThem,

    /// <summary>
    /// 雙方已是好友。
    /// </summary>
    Friends,
}
