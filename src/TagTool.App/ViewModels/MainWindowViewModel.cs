using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TagTool.App.Core.Extensions;
using TagTool.App.Views.UserControls;

namespace TagTool.App.ViewModels;

public interface ISideTool
{
    public string Placement { get; set; }

    public double Width { get; set; }
}

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IFocusManager _focusManager;

    [ObservableProperty]
    private ObservableCollection<ISideTool> _sideTools = new();

    [ObservableProperty]
    private ISideTool? _activeLeftTool;

    [ObservableProperty]
    private ISideTool? _activeRightTool;

    [ObservableProperty]
    private GridLength _activeLeftToolWidth = new(350);

    [ObservableProperty]
    private GridLength _activeRightToolWidth = new(400);

    private bool IsLeftToolPaneOpen => ActiveLeftTool is not null;

    private bool IsRightToolPaneOpen => ActiveRightTool is not null;

    /// <summary>
    ///     ctor for XAML previewer
    /// </summary>
    public MainWindowViewModel()
    {
        _focusManager = null!;
    }

    private InputElement? _previouslyFocusedElement;

    [RelayCommand]
    private void ChangeLeftToolMenuPanelVisibility(bool? isVisible = null)
    {
        ActiveRightToolWidth = isVisible switch
        {
            null => ActiveRightToolWidth == new GridLength(0) ? new GridLength(200) : new GridLength(0),
            true => new GridLength(200),
            false => new GridLength(0)
        };
    }

    [RelayCommand]
    private void ChangeRightToolMenuPanelVisibility(bool? isVisible = null)
    {
        ActiveLeftToolWidth = isVisible switch
        {
            null => ActiveLeftToolWidth == new GridLength(0) ? new GridLength(200) : new GridLength(0),
            true => new GridLength(200),
            false => new GridLength(0),
        };
    }

    [RelayCommand]
    private void ResetFocus(object element)
    {
        _focusManager.Focus((InputElement)element);
        _previouslyFocusedElement = null;
    }

    [RelayCommand]
    private void FocusNextSearchTab(object element)
    {
        var focusTargets = FindAllVisibleSearchBars((Window)element);

        var elementToFocus = focusTargets.Length switch
        {
            0 => null,
            1 => focusTargets[0],
            _ => NextToFocus(focusTargets)
        };

        _previouslyFocusedElement = elementToFocus;
        _focusManager.Focus(elementToFocus);
    }

    private InputElement? NextToFocus(AutoCompleteBox?[] focusTargets)
        => _previouslyFocusedElement is null || !focusTargets.TryFind(box => Equals(box, _previouslyFocusedElement), out var index)
            ? focusTargets[0]
            : focusTargets[index + 1 < focusTargets.Length ? index + 1 : 0];

    private static AutoCompleteBox?[] FindAllVisibleSearchBars(Window rootWindow)
        => rootWindow
            .GetVisualDescendants()
            .Where(logical => logical.GetType() == typeof(SimpleTagsBar) && logical.IsVisible)
            .Select(visual => visual.FindDescendantOfType<AutoCompleteBox>())
            .ToArray();
}
