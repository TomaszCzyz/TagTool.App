using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TagTool.App.ViewModels.UserControls;

public partial class AddressSegmentViewModel : ObservableObject
{
    private readonly DirectoryInfo _folder;

    [ObservableProperty]
    private bool _isPopupOpen;

    public AddressSegmentViewModel(DirectoryInfo folder)
    {
        _folder = folder;
    }

    public string Name => _folder.Name;

    public string Address => _folder.FullName;

    public IEnumerable<AddressSegmentViewModel> Children
        => _folder
            .EnumerateDirectories("*", new EnumerationOptions { IgnoreInaccessible = true })
            .Select(x => new AddressSegmentViewModel(x));

    [RelayCommand]
    public void OpenPopupCommand()
    {
        IsPopupOpen = true;
    }
}
