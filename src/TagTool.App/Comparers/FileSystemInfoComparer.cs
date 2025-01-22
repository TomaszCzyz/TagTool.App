namespace TagTool.App.Core.Comparers;

public class FileSystemInfoComparer : IComparer<FileSystemInfo>
{
    public static IComparer<FileSystemInfo> Comparer { get; } = new FileSystemInfoComparer();

    public int Compare(FileSystemInfo? x, FileSystemInfo? y)
    {
        if (ReferenceEquals(x, y))
        {
            return 0;
        }

        if (ReferenceEquals(null, y))
        {
            return 1;
        }

        if (ReferenceEquals(null, x))
        {
            return -1;
        }

        if ((x.Attributes & FileAttributes.Directory) != 0 && (y.Attributes & FileAttributes.Directory) == 0)
        {
            return 1;
        }

        if ((x.Attributes & FileAttributes.Directory) == 0 && (y.Attributes & FileAttributes.Directory) != 0)
        {
            return -1;
        }

        return string.CompareOrdinal(x.FullName, y.FullName);
    }
}
