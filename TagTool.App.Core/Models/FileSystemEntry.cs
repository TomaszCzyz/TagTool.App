namespace TagTool.App.Core.Models;

public class FileSystemEntry
{
    private readonly FileSystemInfo _info;

    public string Name => _info.Name;

    public string FullName => _info.FullName;

    public string Ext => _info.Extension;

    public long? Length => _info is FileInfo info ? info.Length : null;

    public DateTime DateCreated => _info.CreationTime;

    public DateTime DateModified => _info.LastWriteTime;

    public FileSystemEntry(FileSystemInfo info)
    {
        _info = info;
    }
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
