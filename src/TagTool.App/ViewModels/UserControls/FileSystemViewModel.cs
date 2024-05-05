using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Dock.Model.Mvvm.Controls;
using DynamicData;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.Core;
using TagTool.App.Core.Extensions;
using TagTool.App.Core.Models;
using TagTool.App.Core.Services;
using TagTool.App.Core.ViewModels;
using TagTool.Backend;
using TagTool.Backend.DomainTypes;

namespace TagTool.App.ViewModels.UserControls;

public partial class FileSystemViewModel : Document
{
    private readonly FolderActionsService.FolderActionsServiceClient _folderActionsService;
    private readonly TagService.TagServiceClient _tagService;
    private readonly FileActionsService.FileActionsServiceClient _fileActionsService;
    private readonly Stack<DirectoryInfo> _navigationHistoryBack = new();
    private readonly Stack<DirectoryInfo> _navigationHistoryForward = new();

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
    private bool _areTagsVisible = true;

    public bool CanNavigateBack => _navigationHistoryBack.Count > 0;

    public bool CanNavigateForward => _navigationHistoryForward.Count > 0;

    /// <summary>
    ///     ctor for XAML previewer
    /// </summary>
    public FileSystemViewModel()
    {
        if (!Design.IsDesignMode)
        {
            Debug.Fail("ctor for XAML Previewer should not be invoke during standard execution");
        }

        _fileActionsService = AppTemplate.Current.Services.GetRequiredService<ITagToolBackend>().GetFileActionsService();
        _folderActionsService = AppTemplate.Current.Services.GetRequiredService<ITagToolBackend>().GetFolderActionsService();
        _tagService = AppTemplate.Current.Services.GetRequiredService<ITagToolBackend>().GetTagService();

        Initialize();
    }

    [UsedImplicitly]
    public FileSystemViewModel(ITagToolBackend tagToolBackend)
    {
        _fileActionsService = tagToolBackend.GetFileActionsService();
        _folderActionsService = tagToolBackend.GetFolderActionsService();
        _tagService = tagToolBackend.GetTagService();

        Initialize();
    }

    private void Initialize() => CurrentFolder = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

    partial void OnAreTagsVisibleChanged(bool value)
    {
        foreach (var taggableItemViewModel in Items)
        {
            taggableItemViewModel.AreTagsVisible = value;
        }
    }

    [RelayCommand]
    private async Task DeleteTaggableItem(TaggableItemViewModel vm)
    {
        var isDeleted = vm.TaggableItem switch
        {
            TaggableFile taggableFile => await DeleteTaggableFile(taggableFile),
            TaggableFolder taggableFolder => await DeleteTaggableFolder(taggableFolder),
            _ => false
        };

        if (isDeleted)
        {
            // todo: it does not focus...
            SelectNext();
            Items.Remove(vm);
        }
    }

    private void SelectNext()
    {
        if (SelectedItem is null)
        {
            return;
        }

        var selected = Items.IndexOf(SelectedItem);
        SelectedItem = Items[(selected + 1) % Items.Count];
    }

    private async Task<bool> DeleteTaggableFolder(TaggableFolder taggableFolder)
    {
        var request = new DeleteFolderRequest { Folder = new FolderDto { Path = taggableFolder.Path } };
        var reply = await _folderActionsService.DeleteFolderAsync(request);

        switch (reply.ResultCase)
        {
            case DeleteFolderReply.ResultOneofCase.DeletedFolderFullName:
                WeakReferenceMessenger.Default.Send(
                    new Notification(
                        "Folder deleted successfully",
                        $"Successfully deleted folder {Path.GetFileName(reply.DeletedFolderFullName)}.").ToMessage());

                return true;
            case DeleteFolderReply.ResultOneofCase.ErrorMessage:
                WeakReferenceMessenger.Default.Send(
                    new Notification(
                        "Failed to delete the folder",
                        $"The folder {Path.GetDirectoryName(taggableFolder.Path)} has not been deleted.\nBackend service message:\n{reply.ErrorMessage}",
                        NotificationType.Warning).ToMessage());

                return false;
            default:
                throw new ArgumentOutOfRangeException(reply.ResultCase.ToString());
        }
    }

    private async Task<bool> DeleteTaggableFile(TaggableFile taggableFile)
    {
        var request = new DeleteFileRequest { File = new FileDto { Path = taggableFile.Path } };
        var reply = await _fileActionsService.DeleteFileAsync(request);

        switch (reply.ResultCase)
        {
            case DeleteFileReply.ResultOneofCase.DeletedFileFullName:
                WeakReferenceMessenger.Default.Send(
                    new Notification(
                        "File deleted successfully",
                        $"Successfully deleted file {Path.GetFileName(reply.DeletedFileFullName)}.").ToMessage());

                return true;
            case DeleteFileReply.ResultOneofCase.ErrorMessage:
                WeakReferenceMessenger.Default.Send(
                    new Notification("Failed to delete the file",
                        $"File {Path.GetFileName(taggableFile.Path)} has not been deleted.\nBackend service message:\n{reply.ErrorMessage}",
                        NotificationType.Warning));

                return false;
            default:
                throw new ArgumentOutOfRangeException(reply.ResultCase.ToString());
        }
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
        if (!_navigationHistoryBack.TryPop(out var directory))
        {
            return;
        }

        _navigationHistoryForward.Push(CurrentFolder);

        NavigateBackCommand.NotifyCanExecuteChanged();
        NavigateForwardCommand.NotifyCanExecuteChanged();

        NavigateTo(directory, true);
    }

    [RelayCommand(CanExecute = nameof(CanNavigateForward))]
    private void NavigateForward()
    {
        if (!_navigationHistoryForward.TryPop(out var directory))
        {
            return;
        }

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
        if (!file.Exists)
        {
            return;
        }

        _ = _fileActionsService.OpenFile(new OpenFileRequest { FullFileName = file.FullName });

        if (file.Directory is not null && file.Directory.FullName != CurrentFolder.FullName)
        {
            NavigateTo(file.Directory, false);
        }
    }

    private void NavigateTo(DirectoryInfo folder) => NavigateTo(folder, false);

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

        var folderContent = files.Concat(folders);
        // .OrderByDescending(static entry => entry.TaggableItem.GetType())
        // .ThenBy(static entry => entry.DisplayName);

        Items.Clear();
        Items.AddRange(folderContent);

        AddressSegments.Clear();
        AddressSegments.AddRange(CurrentFolder.GetAncestors().Select(folder => new AddressSegmentViewModel(folder, this)));
    }
}
