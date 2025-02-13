using System.Reflection;
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
            // 移除只读属性
            BsonMapper.Global.ResolveMember += (t, mi, mm) =>
            {
                if (mi is PropertyInfo propertyInfo)
                {
                    // 只读属性没有 setter
                    mm.IsIgnore = propertyInfo.CanWrite == false;
                }
            };

            // 原生Datetime Truncate了 Truncate DateTime in milliseconds

            var appSettingOptions = sp.GetRequiredService<IOptions<ApplicationSetting>>().Value;
            DirectoryHelper.CreateFileDirectoryIfNotExists(appSettingOptions.NosqlDbDataSource);

            // Collation 只会影响查询字符串的排序，所以也不重要，默认：zh-CN/IgnoreCase
            var liteDatabase = new LiteDatabase(new ConnectionString(appSettingOptions.NosqlDbDataSource) { Connection = ConnectionType.Direct, Collation = new Collation("en-US/IgnoreCase") });

            return liteDatabase;
        });

        services.AddLocalNoSQLDBProvidersInjectHostDI(hostEnvironment);

        return services;
    }
}