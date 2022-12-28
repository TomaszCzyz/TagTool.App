using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.ViewModels.UserControls;

namespace TagTool.App.Views.UserControls;

public partial class MyTagsView : UserControl
{
    private readonly MyTagsViewModel _vm = App.Current.Services.GetRequiredService<MyTagsViewModel>();

    public MyTagsView()
    {
        DataContext = _vm;
        InitializeComponent();

        TagsListBox.AddHandler(PointerPressedEvent,  InputElement_OnPointerPressed, handledEventsToo: true);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private async void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_vm.SelectedTag is null) return;

        var dragData = new DataObject();
        dragData.Set("draggedTag", _vm.SelectedTag!);

        var _ = await DragDrop.DoDragDrop(e, dragData, DragDropEffects.Link);
    }
}
