using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Mvvm.Controls;
using DynamicData;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.Core.Extensions;
using TagTool.App.Core.Models;
using TagTool.App.Core.Services;
using TagTool.App.Core.ViewModels;
using TagTool.Backend;

namespace TagTool.App.ViewModels.UserControls;

public partial class FileSystemViewModel : Document
{
    private readonly TagService.TagServiceClient _tagService;
    private readonly FileActionsService.FileActionsServiceClient _fileActionsService;
    private readonly Stack<DirectoryInfo> _navigationHistoryBack = new();
    private readonly Stack<DirectoryInfo> _navigationHistoryForward = new();
    private readonly List<TaggableItemViewModel> _highlightedItems = new();

    [ObservableProperty]
    private ObservableCollection<TaggableItemViewModel> _items = new();

    [ObservableProperty]
    private ObservableCollection<AddressSegmentViewModel> _addressSegments = new();

    [ObservableProperty]
    private DirectoryInfo _currentFolder = new("C:");

    [ObservableProperty]
    private bool _isEditing;

    [EditorBrowsable(EditorBrowsableState.Never)]
    [ObservableProperty]
    private string _addressTextBox = string.Empty;

    [ObservableProperty]
    private TaggableItemViewModel? _selectedItem;

    [ObservableProperty]
    private bool _isQuickSearching;

    [ObservableProperty]
    private string _quickSearchText = "";

    [ObservableProperty]
    private bool _areTagsVisible = true;

    [ObservableProperty]
    private bool _hasQuickSearchResults;

    public bool CanNavigateBack => _navigationHistoryBack.Count > 0;

    public bool CanNavigateForward => _navigationHistoryForward.Count > 0;


    /// <summary>
    ///     Because DataGrid handles KeyDownEvent first, it is inconvenient to use SelectedItem property,
    ///     as it points to the next item, when quick search navigation is performed.
    ///     Also, the SelectedItem is not updated if event occurs on the last item.
    /// </summary>
    private TaggableItemViewModel? _quickSearchSelectedItem;

    /// <summary>
    ///     ctor for XAML previewer
    /// </summary>
    public FileSystemViewModel()
    {
        if (!Design.IsDesignMode)
        {
            Debug.Fail("ctor for XAML Previewer should not be invoke during standard execution");
        }

        _fileActionsService = App.Current.Services.GetRequiredService<ITagToolBackend>().GetFileActionsService();
        _tagService = App.Current.Services.GetRequiredService<ITagToolBackend>().GetTagService();

        Initialize();
    }

    [UsedImplicitly]
    public FileSystemViewModel(ITagToolBackend tagToolBackend)
    {
        _fileActionsService = tagToolBackend.GetFileActionsService();
        _tagService = tagToolBackend.GetTagService();

        Initialize();
    }

    private void Initialize() => CurrentFolder = new DirectoryInfo(@"C:\Users\tczyz\MyFiles");

    partial void OnQuickSearchTextChanged(string value)
    {
        if (value == "")
        {
            _highlightedItems.Clear();
        }

        // todo: add cancellation support in a case of large folders
        Dispatcher.UIThread.Post(() => OnQuickSearchTextChangedInner(value));
    }

    /// <summary>
    ///     Highlights substring that matches <paramref name="value" />
    /// </summary>
    /// <remarks>Invoke on UI thread</remarks>
    /// <param name="value">string we search for</param>
    private void OnQuickSearchTextChangedInner(string value)
    {
        foreach (var entry in Items)
        {
            var index = entry.DisplayName.IndexOf(value, StringComparison.CurrentCultureIgnoreCase);

            if (index < 0)
            {
                if (_highlightedItems.Contains(entry))
                {
                    entry.Inlines.Clear();
                    entry.Inlines.Add(new Run { Text = entry.DisplayName });

                    _highlightedItems.Remove(entry);
                }

                continue;
            }

            entry.Inlines.Clear();
            entry.Inlines.AddRange(CreateHighlightedText(index, index + value.Length, entry.DisplayName));

            if (_highlightedItems.Contains(entry)) continue;
            _highlightedItems.Add(entry);
        }

        UpdateSelectedItem();
    }

    private static IEnumerable<Run> CreateHighlightedText(int begin, int end, string name)
    {
        var solidColorBrush = new SolidColorBrush(Color.FromRgb(177, 127, 55), 0.6);

        yield return new Run { Text = name[..begin] };
        yield return new Run { Text = name[begin..end], FontWeight = FontWeight.Bold, Background = solidColorBrush };
        yield return new Run { Text = name[end..] };
    }

    private void UpdateSelectedItem()
    {
        HasQuickSearchResults = _highlightedItems.Count != 0;

        if (!HasQuickSearchResults) return;

        FocusNextOrCurrentHighlightItem();

        void FocusNextOrCurrentHighlightItem()
        {
            var selectedItemIndex = SelectedItem is not null ? Items.IndexOf(SelectedItem) : 0;

            foreach (var highlightedItem in _highlightedItems.Where(item => Items.IndexOf(item) >= selectedItemIndex))
            {
                _quickSearchSelectedItem = SelectedItem = highlightedItem;
                return;
            }

            _quickSearchSelectedItem = SelectedItem = _highlightedItems.FirstOrDefault();
        }
    }

    partial void OnAreTagsVisibleChanged(bool value)
    {
        foreach (var taggableItemViewModel in Items)
        {
            taggableItemViewModel.AreTagsVisible = value;
        }
    }

    [RelayCommand(CanExecute = nameof(HasQuickSearchResults))]
    private void GoToNextMatchedItem()
    {
        var currentIndex = Items.IndexOf(_quickSearchSelectedItem!);

        foreach (var highlightedItem in _highlightedItems.Where(item => Items.IndexOf(item) > currentIndex))
        {
            _quickSearchSelectedItem = SelectedItem = highlightedItem;
            return;
        }

        _quickSearchSelectedItem = SelectedItem = _highlightedItems.FirstOrDefault();
    }

    [RelayCommand(CanExecute = nameof(HasQuickSearchResults))]
    private void GoToPreviousMatchedItem()
    {
        var currentIndex = Items.IndexOf(_quickSearchSelectedItem!);

        foreach (var highlightedItem in Enumerable.Reverse(_highlightedItems).Where(item => Items.IndexOf(item) < currentIndex))
        {
            _quickSearchSelectedItem = SelectedItem = highlightedItem;
            return;
        }

        _quickSearchSelectedItem = SelectedItem = _highlightedItems.LastOrDefault();
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
        info ??= SelectedItem?.TaggableItem switch
        {
            TaggableFile taggableFile => new FileInfo(taggableFile.Path),
            TaggableFolder taggableFolder => new DirectoryInfo(taggableFolder.Path),
            _ => throw new UnreachableException()
        };

        switch (info)
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

        var _ = _fileActionsService.OpenFile(new OpenFileRequest { FullFileName = file.FullName });

        if (file.Directory is not null && file.Directory.FullName != CurrentFolder.FullName)
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

        var folders = value
            .EnumerateFiles("*", new EnumerationOptions { IgnoreInaccessible = true })
            .Select(info
                => new TaggableItemViewModel(_tagService)
                {
                    TaggableItem = new TaggableFile { Path = info.FullName }, AreTagsVisible = AreTagsVisible
                });

        var files = value
            .EnumerateDirectories("*", new EnumerationOptions { IgnoreInaccessible = true })
            .Select(info
                => new TaggableItemViewModel(_tagService)
                {
                    TaggableItem = new TaggableFolder { Path = info.FullName }, AreTagsVisible = AreTagsVisible
                });

        var folderContent = files
            .Concat(folders);
            // .OrderByDescending(static entry => entry.TaggableItem.GetType())
            // .ThenBy(static entry => entry.DisplayName);

        Items.Clear();
        Items.AddRange(folderContent);

        AddressSegments.Clear();
        AddressSegments.AddRange(CurrentFolder.GetAncestors().Select(folder => new AddressSegmentViewModel(folder, this)));
    }
}
