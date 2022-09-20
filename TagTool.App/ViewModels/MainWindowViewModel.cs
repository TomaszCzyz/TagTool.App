using Dock.Model.Controls;
using Dock.Model.Core;
using ReactiveUI;

namespace TagTool.App.ViewModels;

public class MainWindowViewModel : ReactiveObject
{
    private IRootDock? _layout;

    public IRootDock? Layout
    {
        get => _layout;
        set => this.RaiseAndSetIfChanged(ref _layout, value);
    }

    public MainWindowViewModel()
    {
        var factory = new NotepadFactory();

        Layout = factory.CreateLayout();
        if (Layout is { })
        {
            factory?.InitLayout(Layout);
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
