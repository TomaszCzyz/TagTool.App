using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using TagTool.App.Models;

namespace TagTool.App.Views.Dialogs;

public partial class NameSpecialTagDialog : Window
{
    public NameSpecialTagDialog()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
        AddHandler(KeyDownEvent, OnKeyDown);
    }

    protected override void OnLoaded()
    {
        base.OnLoaded();

        FileNameTextBox.Focus();
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter when !string.IsNullOrEmpty(FileNameTextBox.Text):
                var nameSpecialTag = new NameSpecialTag
                {
                    FileName = FileNameTextBox.Text,
                    CaseSensitive = CaseSensitiveCheckBox.IsChecked ?? false,
                    MatchSubstrings = MatchSubstringsCheckBox.IsChecked ?? false
                };

                Close(nameSpecialTag);
                break;
            case Key.Escape or Key.Enter:
                Close(null);
                break;
        }
    }
}
