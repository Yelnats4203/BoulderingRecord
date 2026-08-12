using System.Text.RegularExpressions;

namespace BoulderingRecordAPI.Validation;

/// <summary>
/// 定義密碼格式規則：至少 8 碼，且需同時包含大寫英文、小寫英文、數字與特殊符號。
/// </summary>
public static class PasswordPolicy
{
    private static readonly Regex Pattern = new(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}$", RegexOptions.Compiled);

    /// <summary>
    /// 密碼格式不符時回傳給使用者的錯誤訊息。
    /// </summary>
    public const string ErrorMessage = "密碼需至少 8 碼，並包含大寫英文、小寫英文、數字與特殊符號。";

    /// <summary>
    /// 檢查密碼是否符合格式規則。
    /// </summary>
    public static bool IsValid(string password) => Pattern.IsMatch(password);
}
