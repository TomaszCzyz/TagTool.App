using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using TagTool.App.ViewModels.UserControls;

namespace TagTool.App.Views.UserControls;

public partial class MyTagsView : UserControl
{
    private MyTagsViewModel ViewModel => (MyTagsViewModel)DataContext!;

    public MyTagsView()
    {
        InitializeComponent();

        TagsListBox.AddHandler(PointerPressedEvent,  InputElement_OnPointerPressed, handledEventsToo: true);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private async void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (ViewModel.SelectedTag is null) return;

        var dragData = new DataObject();
        dragData.Set("draggedTag", ViewModel.SelectedTag!);

        var _ = await DragDrop.DoDragDrop(e, dragData, DragDropEffects.Link);
    }
}
