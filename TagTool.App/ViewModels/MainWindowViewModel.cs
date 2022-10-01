using CommunityToolkit.Mvvm.ComponentModel;
using Dock.Model.Controls;
using Dock.Model.Core;
using TagTool.App.Docks;

namespace TagTool.App.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private IRootDock? _layout;

    public MainWindowViewModel()
    {
        var factory = new NotepadFactory();

        Layout = factory.CreateLayout();
        if (Layout is { })
        {
            factory.InitLayout(Layout);
        }
    }

    public void CloseLayout()
    {
        if (Layout is IDock dock && dock.Close.CanExecute(null))
        {
            dock.Close.Execute(null);
        }
    }
}
