namespace BoulderingRecordAPI.Services;

public interface IVideoStorageService
{
    VideoUploadAuthorization CreateUploadAuthorization(Guid userId);

    Task<bool> ResourceExistsAsync(string publicId, CancellationToken cancellationToken = default);

    string GetSignedPlaybackUrl(string publicId);

    string GetSignedThumbnailUrl(string publicId);
}
