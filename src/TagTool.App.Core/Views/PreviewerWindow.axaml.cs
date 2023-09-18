using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.Core.Models;
using TagTool.App.Core.ViewModels;

namespace TagTool.App.Core.Views;

public partial class PreviewerWindow : Window
{
    private readonly IList<TaggableItem> _items;
    private int _selectedIndex;
    private TaggableItemPreviewerViewModel _taggableItemPreviewerViewModel;

    public PreviewerWindow(IList<TaggableItem> items, int selectedIndex)
    {
        _items = items;
        _selectedIndex = GetArrayIndex(selectedIndex, _items.Count);
        InitializeComponent();

        var screenFromWindow = Screens.ScreenFromWindow(this);
        if (screenFromWindow is not null)
        {
            TaggableItemPreviewerView.MaxHeight = screenFromWindow.Bounds.Size.Height * 0.8;
            TaggableItemPreviewerView.MaxWidth = screenFromWindow.Bounds.Size.Width * 0.8;
            TaggableItemPreviewerView.MinHeight = 250;
            TaggableItemPreviewerView.MinWidth = 400;
        }

        RootWindow.AddHandler(KeyDownEvent, PreviewSequentItem_OnKeyDown);
    }

    private void PreviewSequentItem_OnKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Right:
            case Key.Up:
                NavigateNext();
                e.Handled = true;
                break;
            case Key.Left:
            case Key.Down:
                NavigatePrevious();
                e.Handled = true;
                break;
        }
    }

    private void NavigateNext()
    {
        _selectedIndex = GetArrayIndex(_selectedIndex + 1, _items.Count);
        _taggableItemPreviewerViewModel.Item = _items[_selectedIndex];
    }

    private void NavigatePrevious()
    {
        _selectedIndex = GetArrayIndex(_selectedIndex - 1, _items.Count);
        _taggableItemPreviewerViewModel.Item = _items[_selectedIndex];
    }

    private static int GetArrayIndex(int i, int arrayLength)
    {
        var mod = i % arrayLength;
        return (mod >= 0) ? mod : mod + arrayLength;
    }

    private void Control_OnLoaded(object? sender, RoutedEventArgs e)
    {
        _taggableItemPreviewerViewModel = AppTemplate.Current.Services.GetRequiredService<TaggableItemPreviewerViewModel>();
        _taggableItemPreviewerViewModel.Item = _items[_selectedIndex];

        DataContext = _taggableItemPreviewerViewModel;
    }

    /// <summary>
    ///     Set position of the window to keep windows in the center of a screen. 
    /// </summary>
    private void TaggableItemPreviewerView_OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        Debug.WriteLine($"size has changed from {e.PreviousSize} to {e.NewSize}");
        Debug.WriteLine($"\t\twindows position: {Position}");
        var screen = Screens.ScreenFromWindow(Owner!) ?? Screens.ScreenFromPoint(Position);

        if (screen is null)
        {
            return;
        }

        var width = screen.Bounds.Size.Width / 2.0 - e.NewSize.Width / 2;
        var height = screen.Bounds.Size.Height / 2.0 - e.NewSize.Height / 2;

        Position = new PixelPoint((int)width, (int)height);
    }
}
