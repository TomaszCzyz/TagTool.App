using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.ViewModels.Dialogs;

namespace TagTool.App.Views.Dialogs;

public partial class AdvancedTaggingDialog : Window
{
    public AdvancedTaggingDialog()
    {
        DataContext = App.Current.Services.GetRequiredService<AdvancedTaggingDialogViewModel>();
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
