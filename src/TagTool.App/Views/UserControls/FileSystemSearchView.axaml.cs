using Avalonia.Controls;
using Avalonia.Input;
using TagTool.App.ViewModels.UserControls;

namespace TagTool.App.Views.UserControls;

public partial class FileSystemSearchView : UserControl
{
    private FileSystemSearchViewModel ViewModel => (FileSystemSearchViewModel)DataContext!;

    public FileSystemSearchView()
    {
        InitializeComponent();
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var t = (sender as TextBlock)!;
        var p = e.GetIntermediatePoints(t)[0];

        var clickedDir = FindClickedDir(t, p);
        ViewModel.AddExcludedPathCommand.Execute(clickedDir);
    }

    private static string FindClickedDir(TextBlock t, PointerPoint p)
    {
        var path = t.Text.AsSpan();
        var last = 0;
        while (true)
        {
            var at = path[last..].IndexOf('\\');

            if (at < 0)
            {
                break;
            }

            var rect = t.TextLayout.HitTestTextRange(last, at).First();
            if (rect.Contains(p.Position))
            {
                return t.Text?[..(last + at)] ?? "";
            }

            last += at + 1;
        }

        return "";
    }
}
