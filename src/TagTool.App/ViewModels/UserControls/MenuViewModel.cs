using CommunityToolkit.Mvvm.ComponentModel;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace TagTool.App.ViewModels.UserControls;

public partial class MenuViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _parentWindow;

    [ObservableProperty]
    private bool _isLeftToolMenuToggleButtonChecked;

    [ObservableProperty]
    private bool _isRightToolMenuToggleButtonChecked;

    /// <summary>
    ///     ctor for XAML previewer
    /// </summary>
    public MenuViewModel()
    {
        _parentWindow = App.Current.Services.GetRequiredService<MainWindowViewModel>();
    }

    [UsedImplicitly]
    public MenuViewModel(MainWindowViewModel mainWindowViewModel)
    {
        _parentWindow = mainWindowViewModel;
    }

    partial void OnIsLeftToolMenuToggleButtonCheckedChanged(bool value)
    {
        _parentWindow.ChangeLeftToolMenuPanelVisibilityCommand.Execute(value);
    }

    partial void OnIsRightToolMenuToggleButtonCheckedChanged(bool value)
    {
        _parentWindow.ChangeRightToolMenuPanelVisibilityCommand.Execute(value);
    }
}
