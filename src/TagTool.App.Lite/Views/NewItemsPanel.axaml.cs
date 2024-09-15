using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.Core;
using TagTool.App.Lite.ViewModels;

namespace TagTool.App.Lite.Views;

public partial class NewItemsPanel : UserControl
{
    private readonly NewItemsPanelViewModel _viewModel = AppTemplate.Current.Services.GetRequiredService<NewItemsPanelViewModel>();

    public NewItemsPanel()
    {
        DataContext = _viewModel;

        InitializeComponent();
    }
}
