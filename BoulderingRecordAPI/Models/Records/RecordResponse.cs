namespace BoulderingRecordAPI.Models.Records;

public record RecordResponse(
    Guid Id,
    string? GymName,
    DateTimeOffset UploadedAt,
    int? Difficulty,
    Guid UploaderId,
    string? Note)
{
    public static RecordResponse FromEntity(Entities.Record record) => new(
        record.Id,
        record.GymName,
        record.UploadedAt,
        record.Difficulty,
        record.UploaderId,
        record.Note);
}
