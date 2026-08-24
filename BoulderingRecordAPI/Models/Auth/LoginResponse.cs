namespace BoulderingRecordAPI.Models.Auth;

/// <summary>
/// 登入成功回應。
/// </summary>
/// <param name="Token">JWT token。</param>
/// <param name="ExpiresAt">token 到期時間。</param>
/// <param name="HasEditPermission">是否具有編輯權限。</param>
/// <param name="UserId">登入使用者的唯一識別碼。</param>
/// <param name="Username">登入使用者的名稱。</param>
public record LoginResponse(string Token, DateTimeOffset ExpiresAt, bool HasEditPermission, Guid UserId, string Username);
