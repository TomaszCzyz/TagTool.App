namespace TagTool.App.Extensions;

public static class ArrayExtensions
{
    public static bool TryFind<T>(this T[] array, Predicate<T> match, out int index)
    {
        index = Array.FindIndex(array, match);

        return index != -1;
    }
}
