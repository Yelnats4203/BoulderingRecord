namespace BoulderingRecordAPI.Models.Sends;

/// <summary>
/// 影片播放網址回應。
/// </summary>
/// <param name="PlaybackUrl">Cloudinary 簽章播放網址。</param>
public record VideoPlaybackResponse(string PlaybackUrl);
