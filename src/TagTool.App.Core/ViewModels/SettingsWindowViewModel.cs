using System.Collections.ObjectModel;

namespace TagTool.App.Core.ViewModels;

public class SettingsWindowViewModel : ViewModelBase
{
    public ObservableCollection<string> WatchedLocations { get; } = ["qqweqwe", "qweq"];
}
