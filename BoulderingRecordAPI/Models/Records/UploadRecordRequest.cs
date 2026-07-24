using Microsoft.AspNetCore.Http;

namespace BoulderingRecordAPI.Models.Records;

public record UploadRecordRequest(IFormFile Video, string? GymName, int? Difficulty, string? Note);
