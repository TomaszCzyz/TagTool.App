using Avalonia;
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
        var rootWindow = element as Window
                         ?? throw new InvalidCastException($"Expected parameter to be of type {typeof(Window)}, got {element.GetType()}");

        var focusTargets = FindAllVisibleSearchBars(rootWindow);

        var elementToFocus = focusTargets.Length switch
        {
            0 => null,
            1 => focusTargets[0],
            _ => NextToFocus(focusTargets)
        };

        _previouslyFocusedElement = elementToFocus;
        _focusManager.Focus(elementToFocus);
    }

    private InputElement? NextToFocus(AutoCompleteBox?[] focusTargets) =>
        _previouslyFocusedElement is null || !focusTargets.TryFind(box => Equals(box, _previouslyFocusedElement), out var index)
            ? focusTargets[0]
            : focusTargets[index + 1 < focusTargets.Length ? index + 1 : 0];

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
