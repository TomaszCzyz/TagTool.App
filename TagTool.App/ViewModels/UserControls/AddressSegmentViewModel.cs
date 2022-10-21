using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TagTool.App.ViewModels.UserControls;

public partial class AddressSegmentViewModel : ObservableObject
{
    private static readonly Func<Folder, AddressSegmentViewModel> _createAddressSegment = x => new AddressSegmentViewModel(x);

    private readonly Folder _folder;

    [ObservableProperty]
    private bool _isPopupOpen;

    public AddressSegmentViewModel(Folder folder)
    {
        _folder = folder;
    }

    public string Name => _folder.Name;

    public string Address => _folder.FullPath;

    public IEnumerable<AddressSegmentViewModel> Children => _folder.Folders.Select(_createAddressSegment);

    [RelayCommand]
    public void OpenPopupCommand()
    {
        IsPopupOpen = true;
    }
}

public sealed class Folder : IEquatable<Folder>
{
    private static readonly EnumerationOptions _defaultEnumerationOptions = new();

    private readonly DirectoryInfo _info;

    public Folder(DirectoryInfo info)
    {
        _info = info;
    }

    public string Name => _info.Name;

    public string FullPath => _info.FullName;

    public Folder? Parent => _info.Parent is { } parent ? new Folder(parent) : null;

    public IEnumerable<FileSystemInfo> Entries
    {
        get
        {
            var fileSystemInfos1 = Folders.Select(folder => (FileSystemInfo)folder._info);
            var fileSystemInfos2 = Files.Select(file => (FileSystemInfo)file.Info);
            return fileSystemInfos1.Concat(fileSystemInfos2);
        }
    }

    public IEnumerable<Folder> Folders => GetFolders();

    public IEnumerable<File> Files => GetFiles();

    public Task DeleteAsync()
    {
        _info.Delete(true);
        return Task.CompletedTask;
    }

    public bool Equals(Folder? other) => other is not null && string.Equals(FullPath, other.FullPath, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is Folder folder && Equals(folder);

    public override int GetHashCode() => HashCode.Combine(FullPath);

    public override string ToString() => $"Folder: '{FullPath}'";

    private IEnumerable<Folder> GetFolders() => _info.EnumerateDirectories("*", _defaultEnumerationOptions).Select(NewFolder);

    private IEnumerable<File> GetFiles() => _info.EnumerateFiles("*", _defaultEnumerationOptions).Select(NewFile);

    private static Folder NewFolder(DirectoryInfo folder) => new(folder);

    private static File NewFile(FileInfo file) => new(file);
}

public sealed class File : IEquatable<File>
{
    public FileInfo Info { get; }

    public File(FileInfo info)
    {
        Info = info;
    }

    public string Name => Info.Name;

    public string FullPath => Info.FullName;

    public Task DeleteAsync()
    {
        Info.Delete();
        return Task.CompletedTask;
    }

    public bool Equals(File? other) => other is not null && string.Equals(FullPath, other.FullPath, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is File file && Equals(file);

    public override int GetHashCode() => HashCode.Combine(FullPath);

    public override string ToString() => $"File: '{FullPath}'";
}
