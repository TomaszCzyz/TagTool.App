using Avalonia;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAI.GPT3;
using OpenAI.GPT3.Interfaces;
using OpenAI.GPT3.Managers;
using TagTool.App.Core.Extensions;
using TagTool.App.Core.Services;
using TagTool.App.Core.Services.Previewers;
using TagTool.App.Core.ViewModels;

namespace TagTool.App.Core;

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
        services.AddSingleton<PreviewerFactory>();
        services.AddSingleton<RasterImagePreviewer>();
        services.AddSingleton<UnsupportedFilePreviewer>();
        services.AddTransient<ISpeechToTagSearchService, SpeechToTagSearchService>();
        services
            .AddOptions<OpenAiOptions>()
            .Configure(settings =>
            {
                settings.ApiKey = Environment.GetEnvironmentVariable("OPEN_AI_API_KEY") ?? "throw new InvalidOperationException()";
                settings.DefaultModelId = OpenAI.GPT3.ObjectModels.Models.ChatGpt3_5Turbo;
            });
        services.AddHttpClient<IOpenAIService, OpenAIService>();
        services.AddViewModelsFromAssembly(typeof(ViewModelBase));

        return configuration;
    }
}
