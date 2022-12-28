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
using TagTool.App.Core.Services;
using TagTool.Backend;

namespace TagTool.App.ViewModels.UserControls;

public class DisplayInfo
{
    public FileSystemEntry Entry { get; set; }
    public List<Tag> AsociatedTags { get; set; }
}

public partial class FileSystemViewModel : ViewModelBase
{
    private readonly TagService.TagServiceClient _tagService;
    private readonly Stack<DirectoryInfo> _navigationHistoryBack = new();
    private readonly Stack<DirectoryInfo> _navigationHistoryForward = new();

    public ObservableCollection<DisplayInfo> Items { get; set; } = new();

    public ObservableCollection<string> DetailsItems { get; set; } = new();

    public ObservableCollection<AddressSegmentViewModel> AddressSegments { get; set; } = new();

    [ObservableProperty]
    private DirectoryInfo _currentFolder = new(@"C:\");

    [ObservableProperty]
    private bool _isEditing;

    [EditorBrowsable(EditorBrowsableState.Never)]
    [ObservableProperty]
    private string _addressTextBox = string.Empty;

    [ObservableProperty]
    private DisplayInfo? _selectedItem;

    [ObservableProperty]
    private bool _isQuickSearching;

    [ObservableProperty]
    private string _quickSearchText = "";

    [ObservableProperty]
    private bool _hasQuickSearchResults;

    public bool CanNavigateBack => _navigationHistoryBack.Count > 0;

    public bool CanNavigateForward => _navigationHistoryForward.Count > 0;

    private readonly List<DisplayInfo> _highlightedItems = new();

    /// <summary>
    ///     Because DataGrid handles KeyDownEvent first, it is inconvenient to use SelectedItem property,
    ///     as it points to the next item, when quick search navigation is performed.
    ///     Also, the SelectedItem is not updated if event occurs on the last item.
    /// </summary>
    private DisplayInfo? _quickSearchSelectedItem;

    public FileSystemViewModel(ITagToolBackend tagToolBackend)
    {
        _tagService = tagToolBackend.GetTagService();
        CurrentFolder = new DirectoryInfo(@"C:\Users\tczyz\MyFiles");

        DetailsItems.AddRange(new[] { "qwe", "wer", "ert" });
    }

    partial void OnQuickSearchTextChanged(string value)
    {
        if (value == "")
        {
            _highlightedItems.Clear();
        }

        // todo: add cancellation support in a case of large folders
        Dispatcher.UIThread.Post(() =>
        {
            OnQuickSearchTextChangedInner(value);
        });
    }

    private void OnQuickSearchTextChangedInner(string value)
    {
        foreach (var (info, entry) in Items.Select(info => (info, info.Entry)))
        {
            var index = entry.Name.IndexOf(value, StringComparison.CurrentCultureIgnoreCase);

            if (index < 0)
            {
                if (_highlightedItems.Contains(info))
                {
                    // reset previous highlighting 
                    entry.Inlines.Clear();
                    entry.Inlines.Add(new Run { Text = entry.Name });

                    _highlightedItems.Remove(info);
                }

                continue;
            }

            entry.Inlines.Clear();
            entry.Inlines.AddRange(CreateHighlightedText(index, index + value.Length, entry.Name));

            if (_highlightedItems.Contains(info)) continue;
            _highlightedItems.Add(info);
        }

        UpdateResults();
    }

    private static IEnumerable<Run> CreateHighlightedText(int begin, int end, string name)
    {
        var solidColorBrush = new SolidColorBrush(Color.FromRgb(177, 127, 55), 0.6);

        yield return new Run { Text = name[..begin] };
        yield return new Run { Text = name[begin..end], FontWeight = FontWeight.Bold, Background = solidColorBrush };
        yield return new Run { Text = name[end..] };
    }

    private void UpdateResults()
    {
        HasQuickSearchResults = _highlightedItems.Count != 0;

        if (!HasQuickSearchResults) return;

        FocusNextHighlightItem();

        void FocusNextHighlightItem()
        {
            foreach (var highlightedItem in _highlightedItems)
            {
                var index = Items.IndexOf(highlightedItem);
                var selectedItemIndex = SelectedItem is not null ? Items.IndexOf(SelectedItem) : 0;

                if (index < selectedItemIndex) continue;

                _quickSearchSelectedItem = SelectedItem = highlightedItem;
                return;
            }

            _quickSearchSelectedItem = SelectedItem = _highlightedItems[0];
        }
    }

    [RelayCommand]
    private async Task TagIt((string TagName, FileSystemEntry FileSystemEntry) data)
    {
        var (tagName, fileSystemEntry) = data;
        var tagRequest = fileSystemEntry.IsDir
            ? new TagRequest { TagNames = { tagName }, FolderInfo = new FolderDescription { Path = fileSystemEntry.FullName } }
            : new TagRequest { TagNames = { tagName }, FileInfo = new FileDescription { Path = fileSystemEntry.FullName } };

        var streamingCall = _tagService.Tag();

        await streamingCall.RequestStream.WriteAsync(tagRequest);
        await streamingCall.RequestStream.CompleteAsync();
    }

    [RelayCommand(CanExecute = nameof(HasQuickSearchResults))]
    private void GoToNextMatchedItem()
    {
        var currentIndex = Items.IndexOf(_quickSearchSelectedItem!);

        foreach (var highlightedItem in _highlightedItems.Where(highlightedItem => Items.IndexOf(highlightedItem) > currentIndex))
        {
            SelectedItem = highlightedItem;
            _quickSearchSelectedItem = SelectedItem;
            return;
        }

        SelectedItem = _highlightedItems[0];
        _quickSearchSelectedItem = SelectedItem;
    }

    [RelayCommand(CanExecute = nameof(HasQuickSearchResults))]
    private void GoToPreviousMatchedItem()
    {
        var currentIndex = Items.IndexOf(_quickSearchSelectedItem!);

        for (var index = _highlightedItems.Count - 1; index >= 0; index--)
        {
            var highlightedItem = _highlightedItems[index];

            if (Items.IndexOf(highlightedItem) >= currentIndex) continue;

            _quickSearchSelectedItem = SelectedItem = highlightedItem;
            return;
        }

        _quickSearchSelectedItem = SelectedItem = _highlightedItems[^1];
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
        var destination = info ?? (FileSystemInfo?)SelectedItem.Entry;

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
                .Select(info => new DisplayInfo { Entry = new FileSystemEntry(info), AsociatedTags = new List<Tag>() })
                .OrderByDescending(static info => info.Entry, FileSystemEntryComparer.StaticFileSystemEntryComparer));

        AddressSegments.Clear();
        AddressSegments.AddRange(CurrentFolder.GetAncestors().Select(folder => new AddressSegmentViewModel(folder, this)));
    }
}
