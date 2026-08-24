namespace BoulderingRecordAPI.Entities;

/// <summary>
/// 好友邀請／好友關係實體；同一筆資料在 <see cref="Status"/> 為 <see cref="FriendRequestStatus.Pending"/> 時代表邀請中，
/// 為 <see cref="FriendRequestStatus.Accepted"/> 時代表雙方已成為好友。
/// </summary>
public class FriendRequest
{
    /// <summary>
    /// 資料列唯一識別碼。
    /// </summary>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// 送出邀請者的使用者 ID。
    /// </summary>
    public Guid RequesterId { get; set; }

    /// <summary>
    /// 收到邀請者的使用者 ID。
    /// </summary>
    public Guid AddresseeId { get; set; }

    /// <summary>
    /// 邀請／好友關係狀態。
    /// </summary>
    public FriendRequestStatus Status { get; set; } = FriendRequestStatus.Pending;

    /// <summary>
    /// 邀請建立時間，同時作為成為好友時間的依據。
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
