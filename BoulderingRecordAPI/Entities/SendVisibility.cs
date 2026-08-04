namespace BoulderingRecordAPI.Entities;

/// <summary>
/// 完攀紀錄的可見度。
/// </summary>
public enum SendVisibility
{
    /// <summary>
    /// 僅上傳者本人可見。
    /// </summary>
    Private = 0,

    /// <summary>
    /// 所有人皆可見。
    /// </summary>
    Public = 1,

    /// <summary>
    /// 可透過分享方式提供他人檢視。
    /// </summary>
    Shareable = 2,
}
