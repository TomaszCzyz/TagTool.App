using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using TagTool.App.Core.Extensions;
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
    private string _addressTextBox = string.Empty;

    [ObservableProperty]
    private FileSystemEntry? _selectedItem;

    [ObservableProperty]
    private bool _isQuickSearching;

    [ObservableProperty]
    private string _quickSearchText = "";

    public bool CanNavigateBack => _navigationHistoryBack.Count > 0;

    public bool CanNavigateForward => _navigationHistoryForward.Count > 0;

    public FileSystemViewModel()
    {
        CurrentFolder = new DirectoryInfo(@"C:\Users\tczyz\MyFiles");

        Task.Run(async () =>
        {
            await Task.Delay(1000);
            return QuickSearchText = "tion";
        });

        Task.Run(async () =>
        {
            await Task.Delay(2000);
            return QuickSearchText = "tions";
        });

        Task.Run(async () =>
        {
            await Task.Delay(3000);
            return QuickSearchText = "tionss";
        });
    }

    private readonly List<int> _highlightedItemsIndexes = new();

    partial void OnQuickSearchTextChanged(string? value)
    {
        _selectedHighlightedMatchIndex = null;
        // todo: add cancellation support in case of large folders
        if (value is null) return;

        Dispatcher.UIThread.Post(() =>
        {
            for (var i = 0; i < Items.Count; i++)
            {
                var fileSystemEntry = Items[i];
                var name = fileSystemEntry.Name;
                var index = name.IndexOf(value, StringComparison.OrdinalIgnoreCase);

                if (index <= 0)
                {
                    if (_highlightedItemsIndexes.Contains(i))
                    {
                        fileSystemEntry.Inlines.Clear();
                        fileSystemEntry.Inlines.Add(new Run { Text = fileSystemEntry.Name });

                        _highlightedItemsIndexes.Remove(i);
                    }

                    continue;
                }

                _highlightedItemsIndexes.Add(i);

                var runs = CreateHighlightedInline(index, index + value.Length, name);

                fileSystemEntry.Inlines.Clear();
                fileSystemEntry.Inlines.AddRange(runs);
            }
        });
    }

    private static IEnumerable<Run> CreateHighlightedInline(int begin, int end, string name)
    {
        var solidColorBrush = new SolidColorBrush(Color.FromRgb(177, 127, 55), 0.6);

        yield return new Run { Text = name[..begin] };
        yield return new Run { Text = name[begin..end], FontWeight = FontWeight.Bold, Background = solidColorBrush };
        yield return new Run { Text = name[end..] };
    }

    private int? _selectedHighlightedMatchIndex;

    [RelayCommand]
    private void GoToNextMatchedItem()
    {
        GoToMatchedItem(1);
    }

    [RelayCommand]
    private void GoToPreviousMatchedItem()
    {
        GoToMatchedItem(-1);
    }

    private void GoToMatchedItem(int step)
    {
        if (_highlightedItemsIndexes.Count == 0) return;

        _selectedHighlightedMatchIndex = _selectedHighlightedMatchIndex is null
            ? _highlightedItemsIndexes[0]
            : (_selectedHighlightedMatchIndex + step) % _highlightedItemsIndexes.Count;

        SelectedItem = Items[_selectedHighlightedMatchIndex.Value];
    }

    [RelayCommand]
    private void CancelAddressChange()
    {
        AddressTextBox = CurrentFolder.FullName;
        IsEditing = false;
    }

    [RelayCommand]
    private void CommitAddress()
    {
        if (File.Exists(AddressTextBox))
        {
            NavigateTo(new FileInfo(AddressTextBox));
        }

        if (Directory.Exists(AddressTextBox))
        {
            NavigateTo(new DirectoryInfo(AddressTextBox));
        }

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
    private void Navigate(FileSystemInfo? info = null)
    {
        var destination = info ?? (FileSystemInfo?)SelectedItem;

        switch (destination)
        {
            case null:
                return;
            case DirectoryInfo folder:
                NavigateTo(folder, false);
                return;
            case FileInfo file:
                NavigateTo(file);
                break;
        }
    }

    private void NavigateTo(FileInfo file)
    {
        if (!file.Exists) return;

        using var process = new Process();

        process.StartInfo.FileName = file.FullName;
        process.StartInfo.UseShellExecute = true;

        process.Start();

        if (file.Directory is not null)
        {
            NavigateTo(file.Directory, false);
        }
    }

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

    partial void OnCurrentFolderChanged(DirectoryInfo value)
    {
        QuickSearchText = "";
        AddressTextBox = CurrentFolder.FullName;

        Items.Clear();
        Items.AddRange(
            value
                .EnumerateFileSystemInfos("*", new EnumerationOptions { IgnoreInaccessible = true })
                .Select(info => new FileSystemEntry(info))
                .OrderByDescending(static entry => entry, FileSystemEntryComparer.StaticFileSystemEntryComparer));

        AddressSegments.Clear();
        AddressSegments.AddRange(CurrentFolder.GetAncestors().Select(folder => new AddressSegmentViewModel(folder, this)));
    }
}
