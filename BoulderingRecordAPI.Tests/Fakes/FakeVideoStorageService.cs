using BoulderingRecordAPI.Services;

namespace BoulderingRecordAPI.Tests.Fakes;

public class FakeVideoStorageService(bool resourceExists = true, bool deleteSucceeds = true) : IVideoStorageService
{
    public List<string> DeletedPublicIds { get; } = [];


    public VideoUploadAuthorization CreateUploadAuthorization(Guid userId)
    {
        Guid sendId = Guid.CreateVersion7();
        string publicId = $"sends/{userId}/{sendId}";
        string folder = $"Bouldering/{userId}";
        return new VideoUploadAuthorization(sendId, publicId, folder, "fake-cloud", "fake-api-key", 0, "fake-signature");
    }

    public string BuildFullPublicId(Guid userId, Guid sendId)
        => $"Bouldering/{userId}/sends/{userId}/{sendId}";

    public Task<bool> ResourceExistsAsync(string publicId, CancellationToken cancellationToken = default)
        => Task.FromResult(resourceExists);

    public string GetSignedPlaybackUrl(string publicId) => $"https://fake-cdn.test/{publicId}?token=fake";

    public string GetSignedThumbnailUrl(string publicId) => $"https://fake-cdn.test/{publicId}.jpg?token=fake";

    public Task<bool> DeleteResourceAsync(string publicId, CancellationToken cancellationToken = default)
    {
        DeletedPublicIds.Add(publicId);
        return Task.FromResult(deleteSucceeds);
    }
}
