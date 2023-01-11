using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
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

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter when !string.IsNullOrEmpty(FileNameTextBox.Text):
                e.Handled = true;

                var nameSpecialTag = new NameSpecialTag
                {
                    FileName = FileNameTextBox.Text,
                    CaseSensitive = CaseSensitiveCheckBox.IsChecked ?? false,
                    MatchSubstrings = MatchSubstringsCheckBox.IsChecked ?? false
                };

                Close(nameSpecialTag);
                break;
            case Key.Enter:
                e.Handled = true;

                Close(null);
                break;
            case Key.Escape:
                e.Handled = true;

                Close(null);
                break;
        }
    }

    // private void CancelButton_OnClick(object? sender, RoutedEventArgs e)
    // {
    //     Close();
    // }
    //
    // private void TagButton_OnClick(object? sender, RoutedEventArgs e)
    // {
    //     var simpleTagsBarViewModel = (SimpleTagsBarViewModel)TagsToApplySimpleTagsBar.DataContext!;
    //
    //     Close((SelectFileTextBox.Text, simpleTagsBarViewModel.Tags));
    // }
}
