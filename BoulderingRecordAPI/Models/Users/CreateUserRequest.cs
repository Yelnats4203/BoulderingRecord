namespace BoulderingRecordAPI.Models.Users;

/// <summary>
/// 建立新使用者的請求內容。
/// </summary>
/// <param name="Username">顯示用的使用者名稱。</param>
/// <param name="Acc">登入帳號。</param>
/// <param name="Psw">登入密碼（明碼，伺服器端會雜湊後儲存）。</param>
/// <param name="HasEditPermission">是否賦予新使用者編輯權限。</param>
/// <param name="IsDemoAcc">是否為測試用（demo）帳號。</param>
public record CreateUserRequest(string Username, string Acc, string Psw, bool HasEditPermission, bool IsDemoAcc);
