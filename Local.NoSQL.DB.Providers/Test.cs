using CommunityToolkit.Mvvm.ComponentModel;
using Local.NoSQL.DB.Providers.Bases;

namespace Local.NoSQL.DB.Providers;

public sealed partial class Test : ObservableCacheBase
{
    [ObservableProperty]
    public partial int TestId { get; set; }

    public byte[] Data { get; set; } = GenerateRandomByteArray(102400);

    public static byte[] GenerateRandomByteArray(int size)
    {
        var array = new byte[size];
        Random.Shared.NextBytes(array); // 填充随机字节
        return array;
    }
}