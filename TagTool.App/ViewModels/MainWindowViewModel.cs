using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Dock.Model.Controls;
using Dock.Model.Core;
using ReactiveUI;

namespace TagTool.App.ViewModels;

public class MainWindowViewModel : ReactiveObject
{
    private readonly IFactory? _factory;
    private IRootDock? _layout;

    public IRootDock? Layout
    {
        get => _layout;
        set => this.RaiseAndSetIfChanged(ref _layout, value);
    }

    public MainWindowViewModel()
    {
        _factory = new NotepadFactory();

        Layout = _factory?.CreateLayout();
        if (Layout is { })
        {
            _factory?.InitLayout(Layout);
        }
    }

    private void AddFileViewModel(FileViewModel fileViewModel)
    {
        var files = _factory?.GetDockable<IDocumentDock>("Files");

        if (Layout is null || files is null) return;

        _factory?.AddDockable(files, fileViewModel);
        _factory?.SetActiveDockable(fileViewModel);
        _factory?.SetFocusedDockable(Layout, fileViewModel);
    }

    private static FileViewModel GetUntitledFileViewModel()
    {
        return new FileViewModel
        {
            Path = string.Empty,
            Title = "Untitled",
            Text = "",
            Encoding = Encoding.Default.WebName
        };
    }

    public void CloseLayout()
    {
        if (Layout is IDock dock && dock.Close.CanExecute(null))
        {
            dock.Close.Execute(null);
        }
    }

    public void FileNew()
    {
        var untitledFileViewModel = GetUntitledFileViewModel();
        AddFileViewModel(untitledFileViewModel);
    }

    private Window? GetWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            return desktopLifetime.MainWindow;
        }

        return null;
    }
}
