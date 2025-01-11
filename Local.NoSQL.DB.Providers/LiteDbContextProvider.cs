using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Net.Utilities.Helper.File;
using Net.Utilities.Models;
using SourceGenerator.InjectHostDI;
using Yitter.IdGenerator;

namespace Local.NoSQL.DB.Providers;

public static class LiteDbContextProvider
{
    public static IServiceCollection AddNosqlDbContext(this IServiceCollection services, IHostEnvironment hostEnvironment)
    {
        services.AddSingleton(sp =>
        {
            if (YitIdHelper.IdGenInstance is null)
                YitIdHelper.SetIdGenerator(new IdGeneratorOptions
                {
                    WorkerId = 1,
                    WorkerIdBitLength = 6,
                    SeqBitLength = 6
                });

            BsonMapper.Global.EmptyStringToNull = false;
            BsonMapper.Global.SerializeNullValues = true;
            BsonMapper.Global.EnumAsInteger = true;

            // 原生Datetime Truncate了 Truncate DateTime in milliseconds

            var appSettingOptions = sp.GetRequiredService<IOptions<ApplicationSetting>>().Value;
            DirectoryHelper.CreateFileDirectoryIfNotExists(appSettingOptions.NosqlDbDataSource);

            var liteDatabase = new LiteDatabase(new ConnectionString(appSettingOptions.NosqlDbDataSource) { Connection = ConnectionType.Shared });

            // 将未提交的-log文件写入主数据库
            liteDatabase.Checkpoint();
            // liteDatabase.Rebuild();

            return liteDatabase;
        });

        services.AddLocalNoSQLDBProvidersInjectHostDI(hostEnvironment);

        return services;
    }
}