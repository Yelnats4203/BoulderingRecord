namespace BoulderingRecordAPI.Entities;

public class Record
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string? GymName { get; set; }
    public DateTimeOffset UploadedAt { get; set; }
    public int? Difficulty { get; set; }
    public Guid UploaderId { get; set; }
    public string VideoPath { get; set; } = string.Empty;
    public string? Note { get; set; }
    public RecordVisibility Visibility { get; set; } = RecordVisibility.Private;
}
