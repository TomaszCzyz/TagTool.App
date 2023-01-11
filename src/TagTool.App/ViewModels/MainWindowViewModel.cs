using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Controls;
using Dock.Model.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
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

    private readonly MyDockFactory _dockFactory;

    /// <summary>
    ///     ctor for XAML previewer
    /// </summary>
    public MainWindowViewModel()
    {
        if (!Design.IsDesignMode)
        {
            Debug.Fail("ctor for XAML Previewer should not be invoke during standard execution");
        }

        _focusManager = null!;
        _dockFactory = App.Current.Services.GetRequiredService<MyDockFactory>();

        Initialize();
    }

    [UsedImplicitly]
    public MainWindowViewModel(MyDockFactory factory)
    {
        _focusManager = null!;
        _dockFactory = factory;

        Initialize();
    }

    private void Initialize()
    {
        Layout = _dockFactory.CreateLayout();
        if (Layout is { })
        {
            _dockFactory.InitLayout(Layout);
        }
    }

    [RelayCommand]
    private void AddDocumentToDock(string type)
    {
        if (_dockFactory.LeftDock.IsActive)
        {
            _dockFactory.LeftDock.CreateNewDocumentCommand.Execute(type);
        }
        else if (_dockFactory.RightDock.IsActive)
        {
            _dockFactory.RightDock.CreateNewDocumentCommand.Execute(type);
        }
        else
        {
            _dockFactory.CentralDock.CreateNewDocumentCommand.Execute(type);
        }
    }

    private InputElement? _previouslyFocusedElement;

    [RelayCommand]
    private void ChangeLeftToolMenuPanelVisibility(object param)
    {
        if (param is not bool isVisible) return;

        if (!isVisible)
        {
            HideAllTabs(_dockFactory.LeftDock);
            _dockFactory.CollapseDock(_dockFactory.LeftDock);
        }
        else
        {
            UnhideAllTabs(_dockFactory.LeftDock);
        }
    }

    private void HideAllTabs(IDock dock)
    {
        Debug.Assert(dock.HiddenDockables is not null);
        Debug.Assert(dock.VisibleDockables is not null);

        if (dock.VisibleDockables.Count == 0) return;

        for (var i = 0; i < dock.VisibleDockables.Count; i++)
        {
            dock.HiddenDockables.Add(dock.VisibleDockables[i]);
            dock.VisibleDockables.RemoveAt(i);
        }
    }

    private void UnhideAllTabs(IDock dock)
    {
        Debug.Assert(dock.HiddenDockables is not null);
        Debug.Assert(dock.VisibleDockables is not null);

        if (dock.HiddenDockables.Count == 0) return;

        for (var i = 0; i < dock.HiddenDockables.Count; i++)
        {
            _dockFactory.AddDockable(dock, dock.HiddenDockables[i]);
            _dockFactory.UpdateDockable(dock, dock.Owner);
            // dock.VisibleDockables.Add();
            dock.HiddenDockables.RemoveAt(i);
        }
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
