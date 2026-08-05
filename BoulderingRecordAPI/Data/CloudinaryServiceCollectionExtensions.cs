using BoulderingRecordAPI.Options;
using BoulderingRecordAPI.Services;
using CloudinaryDotNet;

namespace BoulderingRecordAPI.Data;

public static class CloudinaryServiceCollectionExtensions
{
    public static IServiceCollection AddCloudinaryVideoStorage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        IConfigurationSection section = configuration.GetSection(CloudinaryOptions.SectionName);
        if (!section.Exists())
        {
            throw new InvalidOperationException("設定區塊 'Cloudinary' 未設定。");
        }

        foreach (string key in new[] { "CloudName", "ApiKey", "ApiSecret" })
        {
            if (string.IsNullOrWhiteSpace(section[key]))
            {
                throw new InvalidOperationException($"設定值 'Cloudinary:{key}' 未設定。");
            }
        }

        services.Configure<CloudinaryOptions>(section);

        services.AddSingleton(_ =>
        {
            Account account = new Account(section["CloudName"], section["ApiKey"], section["ApiSecret"]);
            Cloudinary cloudinary = new Cloudinary(account);
            cloudinary.Api.Secure = true;
            return cloudinary;
        });

        services.AddScoped<IVideoStorageService, CloudinaryVideoStorageService>();

        return services;
    }
}
