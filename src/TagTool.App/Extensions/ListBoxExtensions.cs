using Avalonia.Controls;

namespace TagTool.App.Core.Extensions;

public static class ListBoxExtensions
{
    public static bool IsLastItemSelected(this ListBox listBox) => listBox.SelectedIndex == listBox.Items.Count - 1;

    public static bool IsFirstItemSelected(this ListBox listBox) => listBox.SelectedIndex == 0;

    public static void SelectFirst(this ListBox listBox) => listBox.SelectedIndex = 0;

    public static void SelectLast(this ListBox listBox) => listBox.SelectedIndex = listBox.Items.Count - 1;
}
