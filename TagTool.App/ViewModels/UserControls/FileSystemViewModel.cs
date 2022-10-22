using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using TagTool.App.Core.Models;

namespace TagTool.App.ViewModels.UserControls;

public partial class FileSystemViewModel : ObservableObject
{
    // private readonly IServiceProvider _serviceProvider = Application.Current?.CreateInstance<IServiceProvider>()!;
    // private readonly IFileIconProvider _fileIconProvider;
    private readonly Stack<DirectoryInfo> _navigationHistoryBack = new();
    private readonly Stack<DirectoryInfo> _navigationHistoryForward = new();

    public ObservableCollection<FileSystemEntry> Items { get; set; } = new();

    public ObservableCollection<AddressSegmentViewModel> AddressSegments { get; set; } = new();

    [ObservableProperty]
    private DirectoryInfo _currentFolder = new(@"C:\");

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private string _address = string.Empty;

    [EditorBrowsable(EditorBrowsableState.Never)]
    [ObservableProperty]
    private string _textBoxAddress = string.Empty;

    [ObservableProperty]
    private object? _selectedItem;

    public bool CanNavigateBack => _navigationHistoryBack.Count > 0;

    public bool CanNavigateForward => _navigationHistoryForward.Count > 0;

    public FileSystemViewModel()
    {
        // _fileIconProvider = _serviceProvider.GetRequiredService<IFileIconProvider>();
        CurrentFolder = new DirectoryInfo(@"C:\Users\tczyz\MyFiles");
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

    [RelayCommand]
    public void OpenItem(FileSystemInfo info)
    {
        if (info is DirectoryInfo directoryInfo)
        {
            NavigateTo(directoryInfo);
            return;
        }

        if (SelectedItem is FileSystemEntry fileSystemEntry)
        {
            NavigateTo(new DirectoryInfo(fileSystemEntry.FullName));
        }
    }

    partial void OnAddressChanged(string value)
    {
        TextBoxAddress = value;
        NavigateToAddress(value);
        AddressSegments.Clear();
        AddressSegments.AddRange(CurrentFolder.GetAncestors().Select(folder => new AddressSegmentViewModel(folder)));
    }

    partial void OnCurrentFolderChanged(DirectoryInfo value)
    {
        Address = value.FullName;
        Items.Clear();
        Items.AddRange(value.EnumerateFileSystemInfos().Select(info => new FileSystemEntry(info)));
    }

    private void NavigateToAddress(string address)
    {
        if (Directory.Exists(address))
        {
            var path = Path.GetFullPath(address);
            var folder = new DirectoryInfo(path);

            NavigateTo(folder);

            return;
        }

        if (File.Exists(address))
        {
            using var process = new Process();

            process.StartInfo.FileName = address;
            process.StartInfo.UseShellExecute = true;

            process.Start();
        }
    }

    private void NavigateTo(DirectoryInfo folder, bool isHistoryNavigation = false)
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
}
