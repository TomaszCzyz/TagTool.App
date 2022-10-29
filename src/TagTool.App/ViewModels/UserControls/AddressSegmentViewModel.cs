using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TagTool.App.ViewModels.UserControls;

public partial class AddressSegmentViewModel : ViewModelBase
{
    private readonly FileSystemViewModel _owner;
    private readonly DirectoryInfo _folder;

    [ObservableProperty]
    private bool _isPopupOpen;

    public string Name => _folder.Name;

    public string Address => _folder.FullName;

    public AddressSegmentViewModel(DirectoryInfo folder, FileSystemViewModel owner)
    {
        _folder = folder;
        _owner = owner;
    }

    public IEnumerable<AddressSegmentViewModel> Children
        => _folder
            .EnumerateDirectories("*", new EnumerationOptions { IgnoreInaccessible = true })
            .Select(x => new AddressSegmentViewModel(x, _owner));

    [RelayCommand]
    private void OpenPopup() => IsPopupOpen = true;

    [RelayCommand]
    private void NavigateHere()
    {
        _owner.NavigateCommand.Execute(_folder);
    }
}
