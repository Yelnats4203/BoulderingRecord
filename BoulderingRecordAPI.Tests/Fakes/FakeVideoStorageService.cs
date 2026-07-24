using BoulderingRecordAPI.Services;
using Microsoft.AspNetCore.Http;

namespace BoulderingRecordAPI.Tests.Fakes;

public class FakeVideoStorageService : IVideoStorageService
{
    public Task<string> SaveAsync(IFormFile video, Guid recordId, CancellationToken cancellationToken = default)
        => Task.FromResult($"fake-storage/{recordId}{Path.GetExtension(video.FileName)}");
}
