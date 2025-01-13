using System.Diagnostics;
using Local.NoSQL.DB.Providers;
using Local.NoSQL.DB.Providers.Implements;
using Local.NoSQL.DB.Providers.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Net.Utilities.Models;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services
            .Configure<ApplicationSetting>(context.Configuration.GetSection(ApplicationSetting.AppSetting))
            .AddNosqlDbContext(context.HostingEnvironment);
    })
    .Build();

var serviceProvider = host.Services;

var cacheProvider = serviceProvider.GetRequiredService<ICacheProvider>();

var timestamp = Stopwatch.GetTimestamp();

var taskList = new List<Task>();
for (var index = 0; index < 1; index++)
{
    taskList.Add(new Task(() =>
    {
        try
        {
            var tests = cacheProvider.GetArray<Test>(CancellationToken.None);
            Console.WriteLine(LiteDbCacheProviderImpl._useLiteDbCount);

            Test[] insert = [.. Enumerable.Range(1, 10).Select(i => new Test { TestId = i, Name = "今天"}).ToArray()];
            var set = cacheProvider.SetArray(insert, CancellationToken.None);
            Console.WriteLine(LiteDbCacheProviderImpl._useLiteDbCount);
            Console.WriteLine(set);

            tests = cacheProvider.GetArray<Test>(CancellationToken.None);
            Console.WriteLine(LiteDbCacheProviderImpl._useLiteDbCount);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }));
}

// taskList执行
taskList.ForEach(t => t.Start());

Task.WaitAll(taskList.ToArray());

Console.WriteLine(Stopwatch.GetElapsedTime(timestamp).TotalSeconds);

cacheProvider.Dispose();
Console.WriteLine("===========================");