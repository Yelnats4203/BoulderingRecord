using BoulderingRecordAPI.Services;
using Microsoft.AspNetCore.Http;

namespace BoulderingRecordAPI.Tests.Fakes;

public class FakeVideoStorageService : IVideoStorageService
{
    public Task<string> SaveAsync(IFormFile video, Guid userId, Guid sendId, CancellationToken cancellationToken = default)
        => Task.FromResult($"fake-storage/{userId}/{sendId}{Path.GetExtension(video.FileName)}");
}
