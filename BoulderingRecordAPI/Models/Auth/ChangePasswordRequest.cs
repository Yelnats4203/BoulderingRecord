namespace BoulderingRecordAPI.Models.Auth;

/// <summary>
/// 修改密碼的請求內容。
/// </summary>
/// <param name="OldPsw">目前的登入密碼（明碼）。</param>
/// <param name="NewPsw">新密碼（明碼，伺服器端會雜湊後儲存）。</param>
public record ChangePasswordRequest(string OldPsw, string NewPsw);
