namespace BoulderingRecordAPI.Models.Users;

/// <summary>
/// 管理員重設指定使用者密碼的請求內容。
/// </summary>
/// <param name="Acc">目標使用者的登入帳號。</param>
/// <param name="NewPsw">新密碼（明碼，伺服器端會雜湊後儲存）。</param>
public record AdminResetPasswordRequest(string Acc, string NewPsw);
