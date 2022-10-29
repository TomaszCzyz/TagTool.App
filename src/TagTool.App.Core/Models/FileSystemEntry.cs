using System.Collections;

namespace TagTool.App.Core.Models;

public class FileSystemEntry
{
    private readonly FileSystemInfo _info;

    public FileSystemEntry(FileSystemInfo info)
    {
        _info = info;
        IsDir = info is DirectoryInfo;
        IsFile = info is FileInfo;
    }

    public bool IsDir { get; }

    public bool IsFile { get; }

    public Type Type => _info.GetType();

    public string Name => _info.Name;

    public string FullName => _info.FullName;

    public string Ext => _info.Extension;

    public long? Length => _info is FileInfo info ? info.Length : null;

    public DateTime DateCreated => _info.CreationTime;

    public DateTime DateModified => _info.LastWriteTime;
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

public static class DirectoryInfoExtensions
{
    public static IEnumerable<DirectoryInfo> GetAncestors(this DirectoryInfo info) => Ancestors(info);

    private static IEnumerable<DirectoryInfo> Ancestors(DirectoryInfo folder)
    {
        if (folder.Parent is { } parent)
        {
            foreach (var segment in Ancestors(parent))
            {
                yield return segment;
            }
        }

        yield return folder;
    }
}
