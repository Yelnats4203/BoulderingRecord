using System.Net;
using BoulderingRecordAPI.Options;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace BoulderingRecordAPI.Services;

public class CloudinaryVideoStorageService(Cloudinary cloudinary, IOptions<CloudinaryOptions> options) : IVideoStorageService
{
    private const string AuthenticatedType = "authenticated";

    private readonly CloudinaryOptions _options = options.Value;

    public VideoUploadAuthorization CreateUploadAuthorization(Guid userId)
    {
        Guid sendId = Guid.CreateVersion7();
        string publicId = $"sends/{userId}/{sendId}";
        string folder = $"Bouldering/{userId}";
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        Dictionary<string, object> parametersToSign = new Dictionary<string, object>
        {
            ["folder"] = folder,
            ["public_id"] = publicId,
            ["timestamp"] = timestamp,
            ["type"] = AuthenticatedType,
        };
        string signature = cloudinary.Api.SignParameters(parametersToSign);

        return new VideoUploadAuthorization(sendId, publicId, folder, _options.CloudName, _options.ApiKey, timestamp, signature);
    }

    public async Task<bool> ResourceExistsAsync(string publicId, CancellationToken cancellationToken = default)
    {
        GetResourceParams getResourceParams = new GetResourceParams(publicId)
        {
            ResourceType = ResourceType.Video,
            Type = AuthenticatedType,
        };

        GetResourceResult result = await cloudinary.GetResourceAsync(getResourceParams, cancellationToken);
        return result.StatusCode == HttpStatusCode.OK;
    }

    public string GetSignedPlaybackUrl(string publicId)
    {
        return cloudinary.Api.Url
            .ResourceType("video")
            .Type(AuthenticatedType)
            .Signed(true)
            .BuildUrl(publicId);
    }

    public string GetSignedThumbnailUrl(string publicId)
    {
        return cloudinary.Api.Url
            .ResourceType("video")
            .Type(AuthenticatedType)
            .Signed(true)
            .Format("jpg")
            .BuildUrl(publicId);
    }

    public async Task<bool> DeleteResourceAsync(string publicId, CancellationToken cancellationToken = default)
    {
        DeletionParams deletionParams = new DeletionParams(publicId)
        {
            ResourceType = ResourceType.Video,
            Type = AuthenticatedType,
        };

        DeletionResult result = await cloudinary.DestroyAsync(deletionParams);
        return result.Result == "ok" || result.Result == "not found";
    }
}
