﻿using System.Diagnostics;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.Core;
using TagTool.App.Core.ViewModels;

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
        if (!Design.IsDesignMode)
        {
            Debug.Fail("ctor for XAML Previewer should not be invoke during standard execution");
        }

        _parentWindow = AppTemplate.Current.Services.GetRequiredService<MainWindowViewModel>();
    }

    [UsedImplicitly]
    public MenuViewModel(MainWindowViewModel mainWindowViewModel)
    {
        _parentWindow = mainWindowViewModel;
    }

    partial void OnIsLeftToolMenuToggleButtonCheckedChanged(bool value) => _parentWindow.ChangeLeftToolMenuPanelVisibilityCommand.Execute(value);

    partial void OnIsRightToolMenuToggleButtonCheckedChanged(bool value) => _parentWindow.ChangeRightToolMenuPanelVisibilityCommand.Execute(value);
}
