using BoulderingRecordAPI.Entities;

namespace BoulderingRecordAPI.Models.Users;

/// <summary>
/// 使用者資訊回應。
/// </summary>
/// <param name="Id">使用者唯一識別碼。</param>
/// <param name="Username">顯示用的使用者名稱。</param>
/// <param name="Acc">登入帳號。</param>
/// <param name="HasEditPermission">是否具有編輯權限。</param>
/// <param name="CreatedAt">帳號建立時間。</param>
public record UserResponse(Guid Id, string Username, string Acc, bool HasEditPermission, DateTime CreatedAt)
{
    public static UserResponse FromEntity(User user) =>
        new(user.Id, user.Username, user.Acc, user.HasEditPermission, user.CreatedAt);
}
