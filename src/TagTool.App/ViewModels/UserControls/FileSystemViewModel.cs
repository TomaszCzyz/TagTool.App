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

    [ObservableProperty]
    private bool _hasQuickSearchResults;

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
            return QuickSearchText = "tionsd";
        });
    }

    private readonly SortedSet<int> _highlightedItemsIndexes = new();

    partial void OnQuickSearchTextChanged(string value)
    {
        // todo: add cancellation support in case of large folders
        Dispatcher.UIThread.Post(() =>
        {
            OnQuickSearchTextChangedInner(value);
        });
    }

    private void OnQuickSearchTextChangedInner(string value)
    {
        for (var i = 0; i < Items.Count; i++)
        {
            var entry = Items[i];
            var index = entry.Name.IndexOf(value, StringComparison.CurrentCultureIgnoreCase);

            if (index < 0)
            {
                if (_highlightedItemsIndexes.Contains(i))
                {
                    // reset previous highlighting 
                    entry.Inlines.Clear();
                    entry.Inlines.Add(new Run { Text = entry.Name });
                    _highlightedItemsIndexes.Remove(i);
                }

                continue;
            }

            entry.Inlines.Clear();
            entry.Inlines.AddRange(CreateHighlightedText(index, index + value.Length, entry.Name));
            _highlightedItemsIndexes.Add(i);
        }

        UpdateResults();
    }

    private void UpdateResults()
    {
        HasQuickSearchResults = _highlightedItemsIndexes.Count != 0;

        if (SelectedItem is null) return;

        var currentIndex = Items.IndexOf(SelectedItem);
        var newIndex = _highlightedItemsIndexes.FirstOrDefault(i => i >= currentIndex, 0);
        SelectedItem = Items[newIndex];
    }

    private static IEnumerable<Run> CreateHighlightedText(int begin, int end, string name)
    {
        var solidColorBrush = new SolidColorBrush(Color.FromRgb(177, 127, 55), 0.6);

        yield return new Run { Text = name[..begin] };
        yield return new Run { Text = name[begin..end], FontWeight = FontWeight.Bold, Background = solidColorBrush };
        yield return new Run { Text = name[end..] };
    }

    [RelayCommand]
    private void GoToNextMatchedItem()
    {
        GoToMatchedItem();
    }

    [RelayCommand]
    private void GoToPreviousMatchedItem()
    {
        GoToMatchedItem(true);
    }

    private void GoToMatchedItem(bool backwards = false)
    {
        // if (_highlightedItemsIndexes.Count == 0) return;
        //
        // var currentlySelectedIndex = SelectedItem is not null ? Items.IndexOf(SelectedItem) : 0;
        //
        // var sortedIndexes = _highlightedItemsIndexes.ToList();
        // var newIndex = sortedIndexes.BinarySearch(currentlySelectedIndex);
        //
        // SelectedItem = Items[sortedIndexes[newIndex < 0 ? -newIndex : newIndex]];
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
