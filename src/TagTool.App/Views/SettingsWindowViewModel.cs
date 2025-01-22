using System.Collections.ObjectModel;

namespace TagTool.App.Views;

public class SettingsWindowViewModel : ViewModelBase
{
    public ObservableCollection<string> WatchedLocations { get; } = ["qqweqwe", "qweq"];
}
