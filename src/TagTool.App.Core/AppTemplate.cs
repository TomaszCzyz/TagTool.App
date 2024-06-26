﻿using Avalonia;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        services.AddViewModelsFromAssembly(typeof(ViewModelBase));

        return configuration;
    }
}
