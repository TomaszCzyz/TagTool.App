using Avalonia;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.Extensions;
using TagTool.App.Services;
using TagTool.App.Views;
using TagTool.TaggableFile;

namespace TagTool.App;

public abstract class AppTemplate : Application
{
    public static new AppTemplate Current => (AppTemplate)Application.Current!;

    public IServiceProvider Services { get; protected set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected static IConfiguration ConfigureCoreServices(ServiceCollection services)
    {
        services.SetupLogging();
        services.AddConfiguration(out var configuration);

        services.AddSingleton<ITagToolBackendConnectionFactory, GrpcChannelFactory>();
        services.AddSingleton<ITagToolBackend, TagToolBackend>();
        services.AddTransient<ISpeechToTagSearchService, SpeechToTagSearchService>();

        services.AddSingleton<TaggableItemDisplayTextResolver>();
        services.AddSingleton<TaggableItemIconResolver>();
        services.AddSingleton<TaggableItemMapper>();

        services.AddSingleton<TaggableFileIconResolver>();
        services.AddSingleton<TaggableFileDisplayTextResolver>();

        services.AddViewModelsFromAssembly(typeof(ViewModelBase));

        return configuration;
    }
}
