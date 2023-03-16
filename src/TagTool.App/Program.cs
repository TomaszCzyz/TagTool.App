using Avalonia;
using Avalonia.Logging;
using Avalonia.Svg.Skia;

namespace TagTool.App;

public static class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    private static AppBuilder BuildAvaloniaApp()
    {
        // todo: check if it is needed in non-debug mode
        GC.KeepAlive(typeof(SvgImageExtension).Assembly);
        GC.KeepAlive(typeof(Avalonia.Svg.Skia.Svg).Assembly);
        var appBuilder = AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
            .LogToTrace(LogEventLevel.Error);

        return appBuilder;
    }
}
