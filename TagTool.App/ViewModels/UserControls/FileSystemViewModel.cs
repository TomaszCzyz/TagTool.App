using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;

namespace TagTool.App.ViewModels.UserControls;

public partial class FileSystemViewModel : ObservableObject
{
    private readonly Stack<Folder> _navigationHistoryBack = new();
    private readonly Stack<Folder> _navigationHistoryForward = new();

    public ObservableCollection<FileSystemInfo> Items { get; set; } = new();

    public ObservableCollection<AddressSegmentViewModel> AddressSegments { get; set; } = new();

    [ObservableProperty]
    private Folder _currentFolder = new(new DirectoryInfo(@"C:\"));

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private string _address = string.Empty;

    [EditorBrowsable(EditorBrowsableState.Never)]
    [ObservableProperty]
    private string _textBoxAddress = string.Empty;

    public bool CanNavigateBack => _navigationHistoryBack.Count > 0;

    public bool CanNavigateForward => _navigationHistoryForward.Count > 0;

    public FileSystemViewModel()
    {
        CurrentFolder = new Folder(new DirectoryInfo(@"C:\Users\tczyz\MyFiles"));
    }

    [RelayCommand]
    private void ChangeAddress(string address) => Address = address;

    [RelayCommand]
    public void CancelNavigation()
    {
        TextBoxAddress = Address;
        IsEditing = false;
    }

    [RelayCommand]
    public void CommitNavigation()
    {
        Address = TextBoxAddress;
        IsEditing = false;
    }

    [RelayCommand]
    public void NavigateUp()
    {
        if (CurrentFolder.Parent is { } parent)
        {
            NavigateTo(parent);
        }
    }

    [RelayCommand(CanExecute = nameof(CanNavigateBack))]
    public void NavigateBack()
    {
        if (!_navigationHistoryBack.TryPop(out var directory)) return;

        _navigationHistoryForward.Push(CurrentFolder);

        NavigateBackCommand.NotifyCanExecuteChanged();
        NavigateForwardCommand.NotifyCanExecuteChanged();

        NavigateTo(directory, true);
    }

    [RelayCommand(CanExecute = nameof(CanNavigateForward))]
    public void NavigateForward()
    {
        if (!_navigationHistoryForward.TryPop(out var directory)) return;

        _navigationHistoryBack.Push(CurrentFolder);

        NavigateBackCommand.NotifyCanExecuteChanged();
        NavigateForwardCommand.NotifyCanExecuteChanged();

        NavigateTo(directory, true);
    }

    public void OpenItem(FileSystemInfo info)
    {
        if (info is DirectoryInfo directoryInfo)
        {
            NavigateTo(new Folder(directoryInfo));
        }
    }

    partial void OnAddressChanged(string value)
    {
        TextBoxAddress = value;
        AddressSegments.Clear();
        AddressSegments.AddRange(GetAddressSegments(value));
        NavigateToAddress(value);
    }

    partial void OnCurrentFolderChanged(Folder value)
    {
        Address = value.FullPath;
        Items.Clear();
        Items.AddRange(value.Entries);
    }

    private void NavigateTo(Folder folder, bool isHistoryNavigation = false)
    {
        if (!CurrentFolder.Equals(folder) && !isHistoryNavigation)
        {
            _navigationHistoryBack.Push(CurrentFolder);
            NavigateBackCommand.NotifyCanExecuteChanged();

            _navigationHistoryForward.Clear();
            NavigateForwardCommand.NotifyCanExecuteChanged();
        }

        CurrentFolder = folder;
    }

    private IEnumerable<AddressSegmentViewModel> GetAddressSegments(string address)
    {
        if (!Directory.Exists(address))
        {
            return Enumerable.Empty<AddressSegmentViewModel>();
        }

        var path = Path.GetFullPath(address);
        var directory = new DirectoryInfo(path);

        return GetAddressSegments(new Folder(directory));
    }

    private IEnumerable<AddressSegmentViewModel> GetAddressSegments(Folder folder)
    {
        if (folder.Parent is { } parent)
        {
            foreach (var segment in GetAddressSegments(parent))
            {
                yield return segment;
            }
        }

        yield return new AddressSegmentViewModel(folder);
    }

    private void NavigateToAddress(string address)
    {
        if (Directory.Exists(address))
        {
            var path = Path.GetFullPath(address);
            var folder = new Folder(new DirectoryInfo(path));

            NavigateTo(folder);

            return;
        }

        if (System.IO.File.Exists(address))
        {
            using var process = new Process();

            process.StartInfo.FileName = address;
            process.StartInfo.UseShellExecute = true;

            process.Start();
        }
    }
}
