using BoulderingRecordAPI.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace BoulderingRecordAPI.Services;

public class LocalVideoStorageService(IOptions<VideoStorageOptions> options) : IVideoStorageService
{
    private readonly VideoStorageOptions _options = options.Value;

    public async Task<string> SaveAsync(IFormFile video, Guid recordId, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_options.Directory);

        string fileName = $"{recordId}{Path.GetExtension(video.FileName)}";
        string filePath = Path.Combine(_options.Directory, fileName);

        await using FileStream stream = new FileStream(filePath, FileMode.Create);
        await video.CopyToAsync(stream, cancellationToken);

        return filePath;
    }
}
