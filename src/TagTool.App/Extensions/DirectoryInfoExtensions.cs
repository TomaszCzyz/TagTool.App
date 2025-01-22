namespace TagTool.App.Core.Extensions;

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
