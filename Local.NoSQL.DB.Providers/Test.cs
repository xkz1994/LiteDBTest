using CommunityToolkit.Mvvm.ComponentModel;
using Local.NoSQL.DB.Providers.Bases;

namespace Local.NoSQL.DB.Providers;

public sealed partial class Test : ObservableCacheBase
{
    [ObservableProperty]
    public partial int TestId { get; set; }

    public string Name { get; set; } = string.Empty;
    
    public string Name1 { get;  } = string.Empty;
    
    public byte[] Data { get; set; } = GenerateRandomByteArray(1024);

    public static byte[] GenerateRandomByteArray(int size)
    {
        var array = new byte[size];
        Random.Shared.NextBytes(array); // 填充随机字节
        return array;
    }
}