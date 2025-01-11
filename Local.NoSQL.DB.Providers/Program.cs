using Local.NoSQL.DB.Providers;
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

var tests = cacheProvider.GetArray<Test>(CancellationToken.None);

Test[] insert = [.. Enumerable.Range(1, 10).Select(i => new Test { TestId = i }).ToArray()];
var set = cacheProvider.SetArray(insert, CancellationToken.None);
Console.WriteLine(set);

tests = cacheProvider.GetArray<Test>(CancellationToken.None);


cacheProvider.Dispose();
Console.WriteLine("===========================");