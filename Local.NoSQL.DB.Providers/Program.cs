using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using CommunityToolkit.Mvvm.ComponentModel;
using Core.Models.Models.Laser.LineCentricity;
using Local.NoSQL.DB.Providers;
using Local.NoSQL.DB.Providers.Implements;
using Local.NoSQL.DB.Providers.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MiniExcelLibs;
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

var laserLineCentricityItemDtos = cacheProvider.GetArray<LaserLineCentricityItemDto>();

foreach (var group in laserLineCentricityItemDtos.Where(t=>t.MicroscopeMagnificationEnum>0).GroupBy(t=>t.StageSpeedEnum))
{
    var valuesX = new List<Dictionary<string, object>>();

    foreach (var laserLineCentricityItemDto in group.OrderBy(x => x.PmtId))
    {
        var dicX = new Dictionary<string, object>()
        {
            { "ID", laserLineCentricityItemDto.PmtId },
            { "Forward DF Center X", laserLineCentricityItemDto.ForwardDarkMachineCenterPosition.X },
            { "Forward DF Center Y", laserLineCentricityItemDto.ForwardDarkMachineCenterPosition.Y },
            { "Reverse DF Y", laserLineCentricityItemDto.ReverseFindDarkMachinePosition.Y },
            { "Reverse DF Center X", laserLineCentricityItemDto.ReverseDarkMachineCenterPosition.X },
            { "Reverse DF Center Y", laserLineCentricityItemDto.ReverseDarkMachineCenterPosition.Y },
        };
        valuesX.Add(dicX);
    }

    MiniExcel.SaveAs($"C:\\Users\\DELL\\Desktop\\Error{group.Key}.xlsx", valuesX, sheetName: "Error");
}


var timestamp = Stopwatch.GetTimestamp();

// var taskList = new List<Task>();
// for (var index = 0; index < 1; index++)
// {
//     taskList.Add(new Task(() =>
//     {
//         try
//         {
//             var tests = cacheProvider.GetArray<Test>();
//             Console.WriteLine(LiteDbCacheProviderImpl._useLiteDbCount);
//
//             Test[] insert = [.. Enumerable.Range(1, 10).Select(i => new Test { TestId = i, Name = "今天你吃饭了么？？？？，对吧，哈哈😄,nihao" }).ToArray()];
//             var set = cacheProvider.SetArray(insert, CancellationToken.None);
//             Console.WriteLine(LiteDbCacheProviderImpl._useLiteDbCount);
//             Console.WriteLine(set);
//
//             tests = cacheProvider.GetArray<Test>();
//
//
//             Console.WriteLine(LiteDbCacheProviderImpl._useLiteDbCount);
//
//             // tests和insert元素是否相等
//             Console.WriteLine(tests.SequenceEqual(insert));
//         }
//         catch (Exception ex)
//         {
//             Console.WriteLine(ex.Message);
//         }
//     }));
// }
//
// // taskList执行
// taskList.ForEach(t => t.Start());
//
// Task.WaitAll(taskList.ToArray());
//
// Console.WriteLine(Stopwatch.GetElapsedTime(timestamp).TotalSeconds);
//
// cacheProvider.Dispose();
// Console.WriteLine("===========================");