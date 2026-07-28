namespace BoulderingRecordAPI.Models.Auth;

/// <summary>
/// 登入請求。
/// </summary>
/// <param name="Acc">登入帳號。</param>
/// <param name="Psw">登入密碼。</param>
public record LoginRequest(string Acc, string Psw);
