using CommunityToolkit.Mvvm.ComponentModel;
using Local.NoSQL.DB.Providers.Bases;

namespace Local.NoSQL.DB.Providers;

public sealed partial class Test : ObservableCacheBase
{
    [ObservableProperty]
    public partial int TestId { get; set; }
}