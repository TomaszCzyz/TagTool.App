using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Dock.Model.Core;
using Dock.Model.Mvvm.Controls;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.Core.Extensions;
using TagTool.App.Core.ViewModels;
using TagTool.App.Docks;
using TagTool.App.Models;
using TagTool.App.Services;
using TagTool.App.ViewModels.UserControls;
using TagTool.App.Views.UserControls;

namespace TagTool.App.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IRecipient<NewNotificationMessage>
{
    private readonly IWorkspaceManager _workspaceManager;
    private InputElement? _previouslyFocusedElement;

    [ObservableProperty]
    private GridLength _activeLeftToolWidth = new(250);

    [ObservableProperty]
    private GridLength _activeRightToolWidth = new(200);

    [ObservableProperty]
    private ObservableCollection<string> _tools
        = new(new[] { "Search", "My Tags", "File Explorer", "Items Tagger", "Tree File Explorer", "New Search Bar", "Tags Relations" });

    [ObservableProperty]
    private IDock? _layout;

    // try this:
    // public IRootDock Layout => DockFactory.RootDock
    public IFactory DockFactory { get; set; }

    public WindowNotificationManager? NotificationManager { get; set; }

    /// <summary>
    ///     ctor for XAML previewer
    /// </summary>
    public MainWindowViewModel()
    {
        if (!Design.IsDesignMode)
        {
            Debug.Fail("ctor for XAML Previewer should not be invoke during standard execution");
        }

        DockFactory = App.Current.Services.GetRequiredService<DefaultDockFactory>();

        Initialize();
    }

    [UsedImplicitly]
    public MainWindowViewModel(IServiceProvider serviceProvider, IWorkspaceManager workspaceManager)
    {
        _workspaceManager = workspaceManager;

        var (rootDock, dockFactory) = workspaceManager.GetLayout();
        dockFactory.InitLayout(rootDock);
        Layout = rootDock;
        DockFactory = dockFactory;

        Initialize();
    }

    public void Receive(NewNotificationMessage message)
        => NotificationManager?.Show(
            new NotificationViewModel
            {
                Title = "Hey There!",
                Message = message.Value.Message,
                NotificationManager = NotificationManager
            });

    private void Initialize() => WeakReferenceMessenger.Default.Register(this);

    [RelayCommand]
    private void ChangeLeftToolMenuPanelVisibility(object param)
    {
        if (param is not bool isVisible)
        {
            return;
        }

        if (!isVisible)
        {
            // _dockFactory.CollapseDock(_dockFactory.LeftDock);
        }
    }

    [RelayCommand]
    private void ChangeRightToolMenuPanelVisibility(bool? isVisible = null)
        => ActiveLeftToolWidth = isVisible switch
        {
            null => ActiveLeftToolWidth == new GridLength(0) ? new GridLength(200) : new GridLength(0),
            true => new GridLength(200),
            false => new GridLength(0)
        };

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

        // _focusManager.Focus(elementToFocus);
    }

    [RelayCommand]
    private void SaveLayout()
    {
        if (Layout is RootDock root)
        {
            _workspaceManager.SaveLayout(root);
        }
    }

    private InputElement? NextToFocus(AutoCompleteBox?[] focusTargets)
        => _previouslyFocusedElement is null || !focusTargets.TryFind(box => Equals(box, _previouslyFocusedElement), out var index)
            ? focusTargets[0]
            : focusTargets[index + 1 < focusTargets.Length ? index + 1 : 0];

    private static AutoCompleteBox?[] FindAllVisibleSearchBars(Window rootWindow)
        => rootWindow
            .GetVisualDescendants()
            .Where(logical => logical.GetType() == typeof(TaggableItemsSearchView) && logical.IsVisible)
            .Select(visual => visual.FindDescendantOfType<AutoCompleteBox>())
            .ToArray();
}
