namespace TagTool.App.Extensions;

public static class SetExtensions
{
    public static bool AddRange<T>(this ISet<T> set, IEnumerable<T> elements)
    {
        var allAdded = true;

        foreach (var elem in elements)
        {
            if (set.Add(elem))
            {
                continue;
            }

            allAdded = false;
        }

        return allAdded;
    }
}
