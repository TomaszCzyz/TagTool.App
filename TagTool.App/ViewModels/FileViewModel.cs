using CommunityToolkit.Mvvm.ComponentModel;
using Dock.Model.Mvvm.Controls;

namespace TagTool.App.ViewModels;

public partial class FileViewModel : Document
{
    [ObservableProperty]
    private string _path = string.Empty;

    [ObservableProperty]
    private string _text = string.Empty;

    [ObservableProperty]
    private string _encoding = string.Empty;
}
