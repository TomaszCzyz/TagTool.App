using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using TagTool.App.Extensions;
using TagTool.App.ViewModels.UserControls;
using TagTool.App.Views.Dialogs;

namespace TagTool.App.Views.UserControls;

public partial class Toolbar : UserControl
{
    private readonly ToolbarViewModel _vm = Application.Current?.CreateInstance<ToolbarViewModel>()!;

    public Toolbar()
    {
        DataContext = _vm;
        InitializeComponent();

        this.Get<Button>("AddFileButton").Click += delegate
        {
            var window = new AddFileDialog { Height = 200, Width = 600, ShowInTaskbar = false };
            _ = window.ShowDialog(GetWindow());
        };
    }

    private Window CreateSampleWindow()
    {
        Button button;
        Button dialogButton;

        var window = new Window
        {
            Height = 200,
            Width = 200,
            Content = new StackPanel
            {
                Spacing = 4,
                Children =
                {
                    new TextBlock { Text = "Hello world!" },
                    (button = new Button
                    {
                        HorizontalAlignment = HorizontalAlignment.Center, Content = "Click to close", IsDefault = true
                    }),
                    (dialogButton = new Button
                    {
                        HorizontalAlignment = HorizontalAlignment.Center, Content = "Dialog", IsDefault = false
                    })
                }
            },
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        button.Click += (_, _) => window.Close();
        dialogButton.Click += (_, _) =>
        {
            var dialog = CreateSampleWindow();
            dialog.Height = 200;
            dialog.ShowDialog(window);
        };

        return window;
    }

    private Window GetWindow() => VisualRoot as Window ?? throw new AggregateException("Invalid Owner");

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
