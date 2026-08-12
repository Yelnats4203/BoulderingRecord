namespace BoulderingRecordAPI.Models.Auth;

/// <summary>
/// 登入成功回應。
/// </summary>
/// <param name="Token">JWT token。</param>
/// <param name="ExpiresAt">token 到期時間。</param>
/// <param name="HasEditPermission">是否具有編輯權限。</param>
public record LoginResponse(string Token, DateTimeOffset ExpiresAt, bool HasEditPermission);
