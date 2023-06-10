using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using TagTool.App.ViewModels.UserControls;

namespace TagTool.App.Views.UserControls;

public partial class TaggableItemsSearchBarView : UserControl
{
    private TaggableItemsSearchBarViewModel ViewModel => (TaggableItemsSearchBarViewModel)DataContext!;

    public TaggableItemsSearchBarView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void SearchBarInputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // _textBox?.Focus();
        // SearchHelperPopup.IsOpen = true;
    }
}
