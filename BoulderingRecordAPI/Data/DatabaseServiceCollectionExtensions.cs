using Microsoft.EntityFrameworkCore;

namespace BoulderingRecordAPI.Data;

public static class DatabaseServiceCollectionExtensions
{
    public static IServiceCollection AddBoulderingRecordDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        DatabaseProvider provider = configuration.GetValue<DatabaseProvider?>("Database:Provider")
            ?? throw new InvalidOperationException("設定值 'Database:Provider' 未設定。");

        string connectionString = configuration.GetConnectionString(provider.ToString())
            ?? throw new InvalidOperationException($"找不到 provider '{provider}' 對應的連線字串。");

        services.AddDbContext<BoulderingRecordDbContext>(options =>
        {
            switch (provider)
            {
                case DatabaseProvider.SqlServer:
                    options.UseSqlServer(connectionString);
                    break;
                case DatabaseProvider.Sqlite:
                    options.UseSqlite(connectionString);
                    break;
                default:
                    throw new InvalidOperationException($"不支援的 provider '{provider}'。");
            }
        });

        return services;
    }
}
