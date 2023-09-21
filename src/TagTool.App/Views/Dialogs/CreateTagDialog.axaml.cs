using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.Core;
using TagTool.App.ViewModels.Dialogs;

namespace TagTool.App.Views.Dialogs;

public partial class CreateTagDialog : Window
{
    public CreateTagDialog()
    {
#if DEBUG
        this.AttachDevTools();
#endif
        DataContext = AppTemplate.Current.Services.GetRequiredService<CreateTagDialogViewModel>();
        InitializeComponent();

        AddHandler(KeyDownEvent, Window_OnKeyDown);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        NewTagNameTextBox.Focus();

        base.OnLoaded(e);
    }

    private void Window_OnKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Escape:
                Close(null!);
                break;
            case Key.Enter when !DataValidationErrors.GetHasErrors(NewTagNameTextBox):
                Close(NewTagNameTextBox.Text);
                break;
        }
    }
}
