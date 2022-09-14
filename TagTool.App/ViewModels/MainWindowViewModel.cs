using Dock.Model.Controls;
using Dock.Model.Core;
using ReactiveUI;
using TagTool.App.Models;

namespace TagTool.App.ViewModels;

public class MainWindowViewModel : ViewModelBase
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
        _factory = new DockFactory(new DemoData());

        // DebugFactoryEvents(_factory);

        Layout = _factory?.CreateLayout();
        if (Layout is { })
        {
            _factory?.InitLayout(Layout);
            if (Layout is { } root)
            {
                root.Navigate.Execute("Home");
            }
        }

        // NewLayout = new RelayCommand(ResetLayout);
    }

    public string Greeting => "Welcome to Avalonia!";
}
