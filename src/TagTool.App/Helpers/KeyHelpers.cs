using Avalonia.Input;

namespace TagTool.App.Helpers;

public static class KeyHelpers
{
    public static bool IsDigitOrLetter(Key key)
    {
        switch (key)
        {
            case Key.D0:
            case Key.D1:
            case Key.D2:
            case Key.D3:
            case Key.D4:
            case Key.D5:
            case Key.D6:
            case Key.D7:
            case Key.D8:
            case Key.D9:
            case Key.A:
            case Key.B:
            case Key.C:
            case Key.D:
            case Key.E:
            case Key.F:
            case Key.G:
            case Key.H:
            case Key.I:
            case Key.J:
            case Key.K:
            case Key.L:
            case Key.M:
            case Key.N:
            case Key.O:
            case Key.P:
            case Key.Q:
            case Key.R:
            case Key.S:
            case Key.T:
            case Key.U:
            case Key.V:
            case Key.W:
            case Key.X:
            case Key.Y:
            case Key.Z:
            case Key.NumPad0:
            case Key.NumPad1:
            case Key.NumPad2:
            case Key.NumPad3:
            case Key.NumPad4:
            case Key.NumPad5:
            case Key.NumPad6:
            case Key.NumPad7:
            case Key.NumPad8:
            case Key.NumPad9:
                return true;
            default:
                return false;
        }
    }
}
