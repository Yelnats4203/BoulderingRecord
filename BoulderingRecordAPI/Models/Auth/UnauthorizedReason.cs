namespace BoulderingRecordAPI.Models.Auth;

/// <summary>
/// 未授權的原因。
/// </summary>
public enum UnauthorizedReason
{
    /// <summary>
    /// 未帶 token，或 token 已過期／無效，須重新登入。
    /// </summary>
    SessionExpired = 0,

    /// <summary>
    /// token 本身有效，但已被其他裝置的新登入取代。
    /// </summary>
    DuplicateLogin = 1,
}
