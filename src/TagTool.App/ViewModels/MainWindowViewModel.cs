﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Controls;
using Dock.Model.Core;
using TagTool.App.Core.Extensions;
using TagTool.App.Docks;
using TagTool.App.Views.UserControls;

namespace TagTool.App.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private IRootDock? _layout;

    private readonly IFocusManager _focusManager;

    /// <summary>
    ///     ctor for XAML previewer
    /// </summary>
    public MainWindowViewModel()
    {
        _focusManager = null!;
    }

    public MainWindowViewModel(NotepadFactory notepadFactory)
    {
        _focusManager = AvaloniaLocator.Current.GetRequiredService<IFocusManager>();

        Layout = notepadFactory.CreateLayout();
        if (Layout is { })
        {
            notepadFactory.InitLayout(Layout);
        }
    }

    private InputElement? _previouslyFocusedElement;

    [RelayCommand]
    private void ResetFocus(object element)
    {
        _focusManager.Focus((InputElement)element);
        _previouslyFocusedElement = null;
    }

    [RelayCommand]
    private void FocusNextSearchTab(object element)
    {
        var rootWindow = element as Window ?? throw new InvalidCastException($"Expected Window, got {element}");
        var focusTargets = FindAllVisibleSearchBars(rootWindow);

        InputElement? elementToFocus;

        switch (focusTargets.Length)
        {
            case 0:
                return;
            case 1:
                elementToFocus = focusTargets[0];
                break;
            default:
                if (_previouslyFocusedElement is null || !focusTargets.TryFind(box => Equals(box, _previouslyFocusedElement), out var index))
                {
                    elementToFocus = focusTargets[0];
                    break;
                }

                elementToFocus = focusTargets[index + 1 < focusTargets.Length ? index + 1 : 0];
                break;
        }

        _previouslyFocusedElement = elementToFocus;
        _focusManager.Focus(elementToFocus);
    }

    private static AutoCompleteBox?[] FindAllVisibleSearchBars(Window rootWindow) =>
        rootWindow
            .GetVisualDescendants()
            .Where(logical => logical.GetType() == typeof(SimpleTagsBar) && logical.IsVisible)
            .Select(visual => visual.FindDescendantOfType<AutoCompleteBox>())
            .ToArray();

    public void CloseLayout()
    {
        if (Layout is IDock dock && dock.Close.CanExecute(null))
        {
            dock.Close.Execute(null);
        }
    }
}
