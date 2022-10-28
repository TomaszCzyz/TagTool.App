using CommunityToolkit.Mvvm.ComponentModel;
using Dock.Model.Controls;
using Dock.Model.Core;
using TagTool.App.Docks;

namespace TagTool.App.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private IRootDock? _layout;

    public MainWindowViewModel()
    {
    }

    public MainWindowViewModel(NotepadFactory notepadFactory)
    {
        Layout = notepadFactory.CreateLayout();
        if (Layout is { })
        {
            notepadFactory.InitLayout(Layout);
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
