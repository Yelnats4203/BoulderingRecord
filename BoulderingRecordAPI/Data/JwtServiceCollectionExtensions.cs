using BoulderingRecordAPI.Options;
using BoulderingRecordAPI.Services;

namespace BoulderingRecordAPI.Data;

public static class JwtServiceCollectionExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var section = configuration.GetSection(JwtSettings.SectionName);
        if (!section.Exists())
        {
            throw new InvalidOperationException("設定區塊 'Jwt' 未設定。");
        }

        if (string.IsNullOrWhiteSpace(section["Key"]))
        {
            throw new InvalidOperationException("設定值 'Jwt:Key' 未設定。");
        }

        services.Configure<JwtSettings>(section);

        services.AddMemoryCache();
        services.AddSingleton<IActiveTokenStore, MemoryActiveTokenStore>();
        services.AddScoped<ITokenService, TokenService>();

        return services;
    }
}
