using BoulderingRecordAPI.Services;

namespace BoulderingRecordAPI.Tests.Fakes;

public class FakeVideoStorageService(bool resourceExists = true) : IVideoStorageService
{
    public VideoUploadAuthorization CreateUploadAuthorization(Guid userId)
    {
        Guid sendId = Guid.CreateVersion7();
        string publicId = $"sends/{userId}/{sendId}";
        return new VideoUploadAuthorization(sendId, publicId, "fake-cloud", "fake-api-key", 0, "fake-signature");
    }

    public Task<bool> ResourceExistsAsync(string publicId, CancellationToken cancellationToken = default)
        => Task.FromResult(resourceExists);

    public string GetSignedPlaybackUrl(string publicId) => $"https://fake-cdn.test/{publicId}?token=fake";
}
