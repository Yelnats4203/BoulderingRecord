namespace BoulderingRecordAPI.Entities;

/// <summary>
/// 使用者帳號實體。
/// </summary>
public class User
{
    /// <summary>
    /// 使用者唯一識別碼。
    /// </summary>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// 顯示用的使用者名稱。
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 登入帳號。
    /// </summary>
    public string Acc { get; set; } = string.Empty;

    /// <summary>
    /// 已雜湊處理的登入密碼。
    /// </summary>
    public string Psw { get; set; } = string.Empty;

    /// <summary>
    /// 帳號建立時間。
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 是否具有編輯權限。
    /// </summary>
    public bool HasEditPermission { get; set; }

    /// <summary>
    /// 是否為測試用（demo）帳號。
    /// </summary>
    public bool IsDemoAcc { get; set; }
}
