using Avalonia;
using Avalonia.Markup.Xaml;

namespace TagTool.App.Core;

public abstract class AppTemplate : Application
{
    public static new AppTemplate Current => (AppTemplate)Application.Current!;

    public IServiceProvider Services { get; protected set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        Services = ConfigureServices();
    }

    private IServiceProvider ConfigureServices()
    {
        throw new NotImplementedException();
    }
}
