using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Controls.Documents;

namespace TagTool.App.Core.Models;

[DebuggerDisplay("{Name}")]
public class FileSystemEntry
{
    private readonly FileSystemInfo _info;

    public FileSystemEntry(FileSystemInfo info)
    {
        _info = info;
        Inlines?.Add(new Run { Text = _info.Name });
        IsDir = info is DirectoryInfo;
        IsFile = info is FileInfo;
    }

    public bool IsDir { get; }

    public bool IsFile { get; }

    public string Name => _info.Name;

    public InlineCollection? Inlines { get; } = new();

    // todo: it should be lazy loaded... and not be here xd
    public ObservableCollection<string> AssociatedTags { get; init; } = new();

    public string FullName => _info.FullName;

    public string Ext => _info.Extension;

    public long? Length => _info is FileInfo info ? info.Length : null;

    public DateTime DateCreated => _info.CreationTime;

    public DateTime DateModified => _info.LastWriteTime;

    public static explicit operator FileSystemInfo?(FileSystemEntry? entry) => entry?._info;
}

public class FileSystemEntryComparer : IComparer
{
    public int Compare(object? x, object? y)
    {
        if (x is not FileSystemEntry xEntry || y is not FileSystemEntry yEntry)
        {
            return System.Collections.Comparer.Default.Compare(x, y);
        }

        return CompareInternal(xEntry, yEntry);
    }

    private sealed class Comparer : IComparer<FileSystemEntry>
    {
        public int Compare(FileSystemEntry? x, FileSystemEntry? y)
        {
            return x switch
            {
                null when y is null => 0,
                null => -1,
                _ when y is null => 1,
                _ => CompareInternal(x, y)
            };
        }
    }

    private static int CompareInternal(FileSystemEntry x, FileSystemEntry y)
    {
        if (x.IsDir && y.IsFile) return 1;
        if (x.IsFile && y.IsDir) return -1;

        return string.CompareOrdinal(x.Name, y.Name);
    }

    public static IComparer<FileSystemEntry> StaticFileSystemEntryComparer { get; } = new Comparer();
}

public class FileSystemInfoComparer : IComparer<FileSystemInfo>
{
    public int Compare(FileSystemInfo? x, FileSystemInfo? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (ReferenceEquals(null, y)) return 1;
        if (ReferenceEquals(null, x)) return -1;

        if ((x.Attributes & FileAttributes.Directory) != 0 && (y.Attributes & FileAttributes.Directory) == 0) return 1;
        if ((x.Attributes & FileAttributes.Directory) == 0 && (y.Attributes & FileAttributes.Directory) != 0) return -1;

        return string.Compare(x.FullName, y.FullName, StringComparison.Ordinal);
    }

    public static IComparer<FileSystemInfo> Comparer { get; } = new FileSystemInfoComparer();
}
