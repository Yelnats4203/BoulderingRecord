namespace BoulderingRecordAPI.Models.Auth;

/// <summary>
/// 換發 token 回應。
/// </summary>
/// <param name="Token">新的 JWT token。</param>
/// <param name="ExpiresAt">新 token 的到期時間。</param>
public record RefreshTokenResponse(string Token, DateTimeOffset ExpiresAt);
