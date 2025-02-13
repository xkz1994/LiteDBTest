using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Local.NoSQL.DB.Providers.Bases;
using Local.NoSQL.DB.Providers.Interfaces;
using Local.NoSQL.DB.Providers.Others;

namespace Core.Models.Models.Laser.LineCentricity;

public enum MicroscopeMagnificationEnum
{
    [Description("5X")]
    Magnification5X,

    [Description("10X")]
    Magnification10X,

    [Description("50X")]
    Magnification50X,

    [Description("100X")]
    Magnification100X,

    [Description("150X")]
    Magnification150X
}

public enum OpticsMagTypeEnum
{
    Low,
    Middle,
    High
}

public enum StageSpeedEnum
{
    Low,
    Middle,
    High
}

public sealed partial class LaserLineCentricityItemDto : ObservableObject, ICacheItem
{
    [ObservableProperty]
    private MicroscopeMagnificationEnum _microscopeMagnificationEnum;

    [ObservableProperty]
    private OpticsMagTypeEnum _opticsMagTypeEnum;

    [ObservableProperty]
    private StageSpeedEnum _stageSpeedEnum;

    [ObservableProperty]
    private int _pmtId;

    [ObservableProperty]
    private Point _findPosition;

    [ObservableProperty]
    private Point _findBrightMachinePosition;

    [ObservableProperty]
    private Point _forwardFindDarkMachinePosition;

    [ObservableProperty]
    private Point _forwardDarkMachineCenterPosition;

    [ObservableProperty]
    private Point _reverseFindDarkMachinePosition;

    [ObservableProperty]
    private Point _reverseDarkMachineCenterPosition;

    [ObservableProperty]
    private string _forwardFilePath = string.Empty;

    [ObservableProperty]
    private string _reverseFilePath = string.Empty;

    [ObservableProperty]
    private string _templateFilePath = string.Empty;

    [ObservableProperty]
    private string _templateImageFilePath = string.Empty;

    public long Id { get; set; }
    public long Expiration { get; set; }
    public DateTime CreatedTime { get; set; }
    public bool IsDeleted { get; set; }
}

public sealed partial class Test : ObservableCacheBase
{
    [ObservableProperty]
    public partial int TestId { get; set; }

    public string Name { get; set; } = string.Empty;
    
    public string Name1 { get;  } = string.Empty;
    
    public byte[] Data { get; set; } = GenerateRandomByteArray(1024);

    private bool Equals(Test other)
    {
        return TestId == other.TestId && Name == other.Name && Name1 == other.Name1;
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is Test other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(TestId, Name, Name1);
    }

    public static byte[] GenerateRandomByteArray(int size)
    {
        var array = new byte[size];
        Random.Shared.NextBytes(array); // 填充随机字节
        return array;
    }
}