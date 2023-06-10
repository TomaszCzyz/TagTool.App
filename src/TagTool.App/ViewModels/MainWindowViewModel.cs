using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Dock.Model.Controls;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.Extensions;
using TagTool.App.Models;
using TagTool.App.Models.Messages;
using TagTool.App.ViewModels.UserControls;
using TagTool.App.Views.UserControls;

namespace TagTool.App.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IRecipient<NewNotificationMessage>
{
    private readonly IFocusManager _focusManager;

    public WindowNotificationManager? NotificationManager { get; set; }

    [ObservableProperty]
    private GridLength _activeLeftToolWidth = new(250);

    [ObservableProperty]
    private GridLength _activeRightToolWidth = new(200);

    [ObservableProperty]
    private ObservableCollection<string> _tools
        = new(new[] { "Search", "My Tags", "File Explorer", "Items Tagger", "Tree File Explorer", "New Search Bar" });

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
        WeakReferenceMessenger.Default.Register(this);

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
            _dockFactory.CollapseDock(_dockFactory.LeftDock);
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

    public void Receive(NewNotificationMessage message)
    {
        // NotificationManager?.Show(new Notification("Welcome", "Avalonia now supports Notifications."));
        NotificationManager?.Show(
            new NotificationViewModel
            {
                Title = "Hey There!", Message = message.Value.Message, NotificationManager = NotificationManager,
            });
    }
}
