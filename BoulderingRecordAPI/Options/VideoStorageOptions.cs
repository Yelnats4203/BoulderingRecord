namespace BoulderingRecordAPI.Options;

public class VideoStorageOptions
{
    public const string SectionName = "VideoStorage";

    public string Directory { get; set; } = "App_Data/videos";
}
