using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Controls;
using TagTool.App.Core.Extensions;
using TagTool.App.Models;
using TagTool.App.Views.UserControls;

namespace TagTool.App.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IFocusManager _focusManager;

    [ObservableProperty]
    private GridLength _activeLeftToolWidth = new(250);

    [ObservableProperty]
    private GridLength _activeRightToolWidth = new(200);

    [ObservableProperty]
    private ObservableCollection<string> _tools = new(new[] { "Search", "My Tags", "File Explorer", "Tree File Explorer" });

    [ObservableProperty]
    private IRootDock? _layout;

    private readonly MyFactory _factory;

    /// <summary>
    ///     ctor for XAML previewer
    /// </summary>
    public MainWindowViewModel()
    {
        Tools.Add("NewTool");
        _focusManager = null!;

        _factory = new MyFactory();

        Layout = _factory.CreateLayout();
        if (Layout is { })
        {
            _factory.InitLayout(Layout);
        }
    }

    [RelayCommand]
    private void AddDocumentToDock(string type)
    {
        if (_factory.LeftDock.IsActive)
        {
            _factory.LeftDock.CreateNewDocumentCommand.Execute(type);
        }
        else if (_factory.RightDock.IsActive)
        {
            _factory.RightDock.CreateNewDocumentCommand.Execute(type);
        }
        else
        {
            _factory.CentralDock.CreateNewDocumentCommand.Execute(type);
        }
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
