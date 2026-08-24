using BoulderingRecordAPI.Entities;
using BoulderingRecordAPI.Models.Sends;
using BoulderingRecordAPI.Services;

namespace BoulderingRecordAPI.Models.Friends;

/// <summary>
/// 好友動態牆的影片項目，包裝影片內容並標示所屬好友。
/// </summary>
/// <param name="FriendUserId">影片上傳好友的使用者 ID。</param>
/// <param name="FriendUsername">影片上傳好友的使用者名稱。</param>
/// <param name="Video">影片紀錄內容。</param>
public record FriendVideoResponse(Guid FriendUserId, string FriendUsername, VideoRecordResponse Video)
{
    /// <summary>
    /// 由 <see cref="Send"/> 實體轉換為回應 DTO。
    /// </summary>
    public static FriendVideoResponse FromEntity(Send send, string friendUsername, IVideoStorageService videoStorageService) =>
        new(send.UploaderId, friendUsername, VideoRecordResponse.FromEntity(send, videoStorageService));
}
