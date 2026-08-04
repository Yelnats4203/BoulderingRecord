using Microsoft.AspNetCore.Http;

namespace BoulderingRecordAPI.Services;

public interface IVideoStorageService
{
    Task<string> SaveAsync(IFormFile video, Guid userId, Guid sendId, CancellationToken cancellationToken = default);
}
