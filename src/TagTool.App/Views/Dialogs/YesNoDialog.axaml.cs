using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace TagTool.App.Views.Dialogs;

public partial class YesNoDialog : Window
{
    public string Question { get; init; } = null!;

    public YesNoDialog()
    {
#if DEBUG
        this.AttachDevTools();
#endif
        InitializeComponent();

        AddHandler(KeyDownEvent, OnKeyDown);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        QuestionTextBlock.Text = Question;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        YesButton.Focus();

        base.OnLoaded(e);
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close(null!);
        }
    }

    private void YesButton_OnClick(object? sender, RoutedEventArgs e)
    {
        (bool Answer, bool Remember) result = (true, RememberChoiceCheckBox.IsChecked!.Value);

        Close(result);
    }

    private void NoButton_OnClick(object? sender, RoutedEventArgs e)
    {
        (bool Answer, bool Remember) result = (false, RememberChoiceCheckBox.IsChecked!.Value);

        Close(result);
    }
}
