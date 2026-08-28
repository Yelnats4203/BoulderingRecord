namespace BoulderingRecordAPI.Services;

public interface IVideoStorageService
{
    VideoUploadAuthorization CreateUploadAuthorization(Guid userId);

    string BuildFullPublicId(Guid userId, Guid sendId);

    Task<bool> ResourceExistsAsync(string publicId, CancellationToken cancellationToken = default);

    string GetSignedPlaybackUrl(string publicId);

    string GetSignedThumbnailUrl(string publicId);

    Task<bool> DeleteResourceAsync(string publicId, CancellationToken cancellationToken = default);
}
