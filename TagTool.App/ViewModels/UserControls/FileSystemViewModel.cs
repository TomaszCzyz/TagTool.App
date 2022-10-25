using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using TagTool.App.Core.Models;

namespace TagTool.App.ViewModels.UserControls;

public partial class FileSystemViewModel : ViewModelBase
{
    private readonly Stack<DirectoryInfo> _navigationHistoryBack = new();
    private readonly Stack<DirectoryInfo> _navigationHistoryForward = new();

    public ObservableCollection<FileSystemEntry> Items { get; set; } = new();

    public ObservableCollection<AddressSegmentViewModel> AddressSegments { get; set; } = new();

    [ObservableProperty]
    private DirectoryInfo _currentFolder = new(@"C:\");

    [ObservableProperty]
    private bool _isEditing;

    [EditorBrowsable(EditorBrowsableState.Never)]
    [ObservableProperty]
    private string _textBoxAddress = string.Empty;

    [ObservableProperty]
    private object? _selectedItem;

    public bool CanNavigateBack => _navigationHistoryBack.Count > 0;

    public bool CanNavigateForward => _navigationHistoryForward.Count > 0;

    public FileSystemViewModel()
    {
        CurrentFolder = new DirectoryInfo(@"C:\Users\tczyz\MyFiles");
    }

    [RelayCommand]
    private void CancelNavigation()
    {
        TextBoxAddress = CurrentFolder.FullName;
        IsEditing = false;
    }

    [RelayCommand]
    private void CommitNavigation()
    {
        if (File.Exists(TextBoxAddress))
        {
            using var process = new Process();

            process.StartInfo.FileName = TextBoxAddress;
            process.StartInfo.UseShellExecute = true;

            process.Start();

            NavigateTo(new DirectoryInfo(Path.GetDirectoryName(TextBoxAddress)!));
            IsEditing = false;
            return;
        }

        if (Directory.Exists(TextBoxAddress))
        {
            NavigateTo(new DirectoryInfo(TextBoxAddress));
            IsEditing = false;
            return;
        }

        TextBoxAddress = CurrentFolder.FullName;
        IsEditing = false;
    }

    [RelayCommand]
    private void NavigateUp()
    {
        if (CurrentFolder.Parent is { } parent)
        {
            NavigateTo(parent);
        }
    }

    [RelayCommand(CanExecute = nameof(CanNavigateBack))]
    private void NavigateBack()
    {
        if (!_navigationHistoryBack.TryPop(out var directory)) return;

        _navigationHistoryForward.Push(CurrentFolder);

        NavigateBackCommand.NotifyCanExecuteChanged();
        NavigateForwardCommand.NotifyCanExecuteChanged();

        NavigateTo(directory, true);
    }

    [RelayCommand(CanExecute = nameof(CanNavigateForward))]
    private void NavigateForward()
    {
        if (!_navigationHistoryForward.TryPop(out var directory)) return;

        _navigationHistoryBack.Push(CurrentFolder);

        NavigateBackCommand.NotifyCanExecuteChanged();
        NavigateForwardCommand.NotifyCanExecuteChanged();

        NavigateTo(directory, true);
    }

    [RelayCommand]
    private void OpenItem()
    {
        if (SelectedItem is FileSystemEntry { IsDir: true } fileSystemEntry)
        {
            NavigateTo(new DirectoryInfo(fileSystemEntry.FullName));
        }
    }

    partial void OnCurrentFolderChanged(DirectoryInfo value)
    {
        TextBoxAddress = CurrentFolder.FullName;

        Items.Clear();
        Items.AddRange(value.EnumerateFileSystemInfos().Select(info => new FileSystemEntry(info)));

        AddressSegments.Clear();
        AddressSegments.AddRange(CurrentFolder.GetAncestors().Select(folder => new AddressSegmentViewModel(folder, this)));
    }

    [RelayCommand]
    private void NavigateTo(DirectoryInfo folder)
    {
        NavigateTo(folder, false);
    }

    private void NavigateTo(DirectoryInfo folder, bool isHistoryNavigation)
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
