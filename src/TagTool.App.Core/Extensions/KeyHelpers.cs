using Avalonia.Input;
using static Avalonia.Input.Key;

namespace TagTool.App.Core.Extensions;

public static class KeyExtensions
{
    public static bool IsDigitOrLetter(this Key key) => key is >= D0 and <= Z or >= NumPad0 and NumPad9;
}
